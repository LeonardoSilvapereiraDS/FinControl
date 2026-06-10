using FinControl.Application.Contas;
using FinControl.Domain.Enums;
using FinControl.Infrastructure.Persistence;

namespace FinControl.WinForms;

public sealed class ContasControl : UserControl
{
    private readonly IContaService _contaService;
    private readonly BindingSource _bindingSource = new();
    private readonly DataGridView _grid = new();
    private readonly TextBox _nomeTextBox = new();
    private readonly ComboBox _tipoComboBox = new();
    private readonly NumericUpDown _saldoInicialInput = new();
    private readonly CheckBox _incluirInativasCheckBox = new();
    private readonly Label _statusLabel = new();
    private int? _contaSelecionadaId;

    public event EventHandler? DadosAlterados;

    public ContasControl(IContaService contaService)
    {
        _contaService = contaService;

        ConfigurarTela();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        try
        {
            var contas = await _contaService.ListarAsync(
                BancoDadosInicializador.UsuarioPadraoId,
                _incluirInativasCheckBox.Checked);

            _bindingSource.DataSource = contas;
            LimparFormulario();
            _statusLabel.Text = $"{contas.Count} contas";
        }
        catch (Exception ex)
        {
            ExibirErro(ex);
        }
    }

    private async Task SalvarAsync()
    {
        try
        {
            var request = new SalvarContaRequest(
                _nomeTextBox.Text,
                (TipoConta)_tipoComboBox.SelectedItem!,
                _saldoInicialInput.Value);

            if (_contaSelecionadaId is int contaId)
            {
                await _contaService.AtualizarAsync(
                    BancoDadosInicializador.UsuarioPadraoId,
                    contaId,
                    request);
            }
            else
            {
                await _contaService.CriarAsync(
                    BancoDadosInicializador.UsuarioPadraoId,
                    request);
            }

            await CarregarAsync();
            DadosAlterados?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ExibirErro(ex);
        }
    }

    private async Task DesativarAsync()
    {
        if (_contaSelecionadaId is not int contaId)
        {
            return;
        }

        try
        {
            await _contaService.DesativarAsync(BancoDadosInicializador.UsuarioPadraoId, contaId);
            await CarregarAsync();
            DadosAlterados?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ExibirErro(ex);
        }
    }

    private async Task ReativarAsync()
    {
        if (_contaSelecionadaId is not int contaId)
        {
            return;
        }

        try
        {
            await _contaService.ReativarAsync(BancoDadosInicializador.UsuarioPadraoId, contaId);
            await CarregarAsync();
            DadosAlterados?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ExibirErro(ex);
        }
    }

    private void ConfigurarTela()
    {
        BackColor = Color.FromArgb(246, 248, 250);

        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = BackColor
        };

        container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
        container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        container.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

        container.Controls.Add(CriarFormulario(), 0, 0);
        container.Controls.Add(CriarGrid(), 1, 0);
        container.Controls.Add(_statusLabel, 1, 1);

        _statusLabel.AutoSize = true;
        _statusLabel.ForeColor = Color.FromArgb(88, 96, 105);
        _statusLabel.Text = "Carregando dados...";

        Controls.Add(container);
    }

    private Control CriarFormulario()
    {
        var painel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 11,
            Padding = new Padding(0, 0, 18, 0),
            BackColor = Color.Transparent
        };

        for (var i = 0; i < painel.RowCount; i++)
        {
            painel.RowStyles.Add(new RowStyle(SizeType.Absolute, i is 0 ? 44 : 42));
        }

        _tipoComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _tipoComboBox.Items.AddRange(Enum.GetValues<TipoConta>().Cast<object>().ToArray());
        SelecionarPrimeiroItem(_tipoComboBox);

        _saldoInicialInput.DecimalPlaces = 2;
        _saldoInicialInput.Maximum = 1_000_000_000;
        _saldoInicialInput.Minimum = -1_000_000_000;
        _saldoInicialInput.ThousandsSeparator = true;
        _saldoInicialInput.Increment = 100;

        var salvarButton = CriarBotao("Salvar");
        var novoButton = CriarBotao("Novo");
        var desativarButton = CriarBotao("Desativar");
        var reativarButton = CriarBotao("Reativar");

        salvarButton.Click += async (_, _) => await SalvarAsync();
        novoButton.Click += (_, _) => LimparFormulario();
        desativarButton.Click += async (_, _) => await DesativarAsync();
        reativarButton.Click += async (_, _) => await ReativarAsync();
        _incluirInativasCheckBox.CheckedChanged += async (_, _) => await CarregarAsync();

        painel.Controls.Add(CriarTitulo("Conta"), 0, 0);
        painel.Controls.Add(CriarCampo("Nome", _nomeTextBox), 0, 1);
        painel.Controls.Add(CriarCampo("Tipo", _tipoComboBox), 0, 2);
        painel.Controls.Add(CriarCampo("Saldo inicial", _saldoInicialInput), 0, 3);
        painel.Controls.Add(salvarButton, 0, 4);
        painel.Controls.Add(novoButton, 0, 5);
        painel.Controls.Add(desativarButton, 0, 6);
        painel.Controls.Add(reativarButton, 0, 7);
        painel.Controls.Add(_incluirInativasCheckBox, 0, 8);

        _incluirInativasCheckBox.Text = "Inativas";
        _incluirInativasCheckBox.AutoSize = true;
        _incluirInativasCheckBox.Margin = new Padding(0, 8, 0, 0);

        return painel;
    }

    private Control CriarGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.MultiSelect = false;
        _grid.ReadOnly = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.RowHeadersVisible = false;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.FixedSingle;
        _grid.DataSource = _bindingSource;

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Nome",
            DataPropertyName = nameof(ContaDto.Nome),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 38
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Tipo",
            DataPropertyName = nameof(ContaDto.TipoConta),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 28
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Saldo inicial",
            DataPropertyName = nameof(ContaDto.SaldoInicial),
            DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 22
        });

        _grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            HeaderText = "Ativa",
            DataPropertyName = nameof(ContaDto.Ativa),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 12
        });

        _grid.SelectionChanged += (_, _) => PreencherFormularioComSelecao();

        return _grid;
    }

    private void PreencherFormularioComSelecao()
    {
        if (_grid.SelectedRows.Count == 0 ||
            _grid.SelectedRows[0].DataBoundItem is not ContaDto conta)
        {
            return;
        }

        _contaSelecionadaId = conta.Id;
        _nomeTextBox.Text = conta.Nome;
        _tipoComboBox.SelectedItem = conta.TipoConta;
        _saldoInicialInput.Value = conta.SaldoInicial;
    }

    private void LimparFormulario()
    {
        _contaSelecionadaId = null;
        _nomeTextBox.Clear();
        SelecionarPrimeiroItem(_tipoComboBox);
        _saldoInicialInput.Value = 0;
        _grid.ClearSelection();
    }

    private static Control CriarCampo(string rotulo, Control controle)
    {
        var painel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 0, 0, 8)
        };

        painel.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        painel.Controls.Add(new Label
        {
            AutoSize = true,
            ForeColor = Color.FromArgb(88, 96, 105),
            Text = rotulo
        }, 0, 0);

        controle.Dock = DockStyle.Fill;
        painel.Controls.Add(controle, 0, 1);

        return painel;
    }

    private static Label CriarTitulo(string texto)
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Color.FromArgb(31, 35, 40),
            Text = texto
        };
    }

    private static Button CriarBotao(string texto)
    {
        return new Button
        {
            Dock = DockStyle.Fill,
            Height = 34,
            Margin = new Padding(0, 0, 0, 8),
            Text = texto,
            UseVisualStyleBackColor = true
        };
    }

    private static void SelecionarPrimeiroItem(ComboBox comboBox)
    {
        if (comboBox.Items.Count > 0)
        {
            comboBox.SelectedIndex = 0;
        }
    }

    private static void ExibirErro(Exception ex)
    {
        MessageBox.Show(
            ex.Message,
            "FinControl",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }
}
