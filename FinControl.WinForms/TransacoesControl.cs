using FinControl.Application.Categorias;
using FinControl.Application.Contas;
using FinControl.Application.Transacoes;
using FinControl.Domain.Enums;
using FinControl.Infrastructure.Persistence;

namespace FinControl.WinForms;

public sealed class TransacoesControl : UserControl
{
    private readonly ICategoriaService _categoriaService;
    private readonly IContaService _contaService;
    private readonly ITransacaoService _transacaoService;
    private readonly BindingSource _bindingSource = new();
    private readonly DataGridView _grid = new();
    private readonly TextBox _descricaoTextBox = new();
    private readonly NumericUpDown _valorInput = new();
    private readonly DateTimePicker _dataInput = new();
    private readonly ComboBox _tipoComboBox = new();
    private readonly ComboBox _categoriaComboBox = new();
    private readonly ComboBox _contaComboBox = new();
    private readonly TextBox _observacaoTextBox = new();
    private readonly CheckBox _pagoCheckBox = new();
    private readonly CheckBox _recorrenteCheckBox = new();
    private readonly Label _statusLabel = new();
    private IReadOnlyList<CategoriaDto> _categorias = [];
    private IReadOnlyList<ContaDto> _contas = [];
    private int? _transacaoSelecionadaId;

    public event EventHandler? DadosAlterados;

    public TransacoesControl(
        ICategoriaService categoriaService,
        IContaService contaService,
        ITransacaoService transacaoService)
    {
        _categoriaService = categoriaService;
        _contaService = contaService;
        _transacaoService = transacaoService;

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
            _categorias = await _categoriaService.ListarAsync(BancoDadosInicializador.UsuarioPadraoId);
            _contas = await _contaService.ListarAsync(BancoDadosInicializador.UsuarioPadraoId);

            AtualizarCategoriaComboBox();
            AtualizarContaComboBox();

            await CarregarTransacoesAsync();
            LimparFormulario();
        }
        catch (Exception ex)
        {
            ExibirErro(ex);
        }
    }

    private async Task CarregarTransacoesAsync()
    {
        var transacoes = await _transacaoService.ListarAsync(BancoDadosInicializador.UsuarioPadraoId);

        _bindingSource.DataSource = transacoes;
        _statusLabel.Text = $"{transacoes.Count} transacoes";
    }

    private async Task SalvarAsync()
    {
        try
        {
            var request = new SalvarTransacaoRequest(
                _descricaoTextBox.Text,
                _valorInput.Value,
                _dataInput.Value.Date,
                (TipoTransacao)_tipoComboBox.SelectedItem!,
                ObterCategoriaSelecionadaId(),
                ObterContaSelecionadaId(),
                _observacaoTextBox.Text,
                _pagoCheckBox.Checked,
                _recorrenteCheckBox.Checked);

            if (_transacaoSelecionadaId is int transacaoId)
            {
                await _transacaoService.AtualizarAsync(
                    BancoDadosInicializador.UsuarioPadraoId,
                    transacaoId,
                    request);
            }
            else
            {
                await _transacaoService.CriarAsync(
                    BancoDadosInicializador.UsuarioPadraoId,
                    request);
            }

            await CarregarTransacoesAsync();
            LimparFormulario();
            DadosAlterados?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ExibirErro(ex);
        }
    }

    private async Task RemoverAsync()
    {
        if (_transacaoSelecionadaId is not int transacaoId)
        {
            return;
        }

        var resultado = MessageBox.Show(
            "Remover transacao selecionada?",
            "FinControl",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (resultado != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _transacaoService.RemoverAsync(BancoDadosInicializador.UsuarioPadraoId, transacaoId);
            await CarregarTransacoesAsync();
            LimparFormulario();
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

        container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360));
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
            RowCount = 15,
            Padding = new Padding(0, 0, 18, 0),
            BackColor = Color.Transparent
        };

        for (var i = 0; i < painel.RowCount; i++)
        {
            painel.RowStyles.Add(new RowStyle(SizeType.Absolute, i is 0 ? 44 : 42));
        }

        _tipoComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _tipoComboBox.Items.AddRange(Enum.GetValues<TipoTransacao>().Cast<object>().ToArray());
        SelecionarPrimeiroItem(_tipoComboBox);
        _tipoComboBox.SelectedIndexChanged += (_, _) => AtualizarCategoriaComboBox();

        _categoriaComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _categoriaComboBox.DisplayMember = nameof(CategoriaDto.Nome);
        _categoriaComboBox.ValueMember = nameof(CategoriaDto.Id);

        _contaComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _contaComboBox.DisplayMember = nameof(ContaDto.Nome);
        _contaComboBox.ValueMember = nameof(ContaDto.Id);

        _valorInput.DecimalPlaces = 2;
        _valorInput.Maximum = 1_000_000_000;
        _valorInput.ThousandsSeparator = true;
        _valorInput.Increment = 10;

        _dataInput.Format = DateTimePickerFormat.Short;
        _dataInput.Value = DateTime.Today;

        _observacaoTextBox.Multiline = true;
        _observacaoTextBox.Height = 54;

        _pagoCheckBox.Text = "Pago";
        _pagoCheckBox.Checked = true;
        _pagoCheckBox.AutoSize = true;

        _recorrenteCheckBox.Text = "Recorrente";
        _recorrenteCheckBox.AutoSize = true;

        var salvarButton = CriarBotao("Salvar");
        var novoButton = CriarBotao("Novo");
        var removerButton = CriarBotao("Remover");

        salvarButton.Click += async (_, _) => await SalvarAsync();
        novoButton.Click += (_, _) => LimparFormulario();
        removerButton.Click += async (_, _) => await RemoverAsync();

        painel.Controls.Add(CriarTitulo("Transacao"), 0, 0);
        painel.Controls.Add(CriarCampo("Descricao", _descricaoTextBox), 0, 1);
        painel.Controls.Add(CriarCampo("Tipo", _tipoComboBox), 0, 2);
        painel.Controls.Add(CriarCampo("Categoria", _categoriaComboBox), 0, 3);
        painel.Controls.Add(CriarCampo("Conta", _contaComboBox), 0, 4);
        painel.Controls.Add(CriarCampo("Valor", _valorInput), 0, 5);
        painel.Controls.Add(CriarCampo("Data", _dataInput), 0, 6);
        painel.Controls.Add(_pagoCheckBox, 0, 7);
        painel.Controls.Add(_recorrenteCheckBox, 0, 8);
        painel.Controls.Add(CriarCampo("Observacao", _observacaoTextBox), 0, 9);
        painel.Controls.Add(salvarButton, 0, 11);
        painel.Controls.Add(novoButton, 0, 12);
        painel.Controls.Add(removerButton, 0, 13);

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
            HeaderText = "Data",
            DataPropertyName = nameof(TransacaoDto.Data),
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" },
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 14
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Descricao",
            DataPropertyName = nameof(TransacaoDto.Descricao),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 30
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Tipo",
            DataPropertyName = nameof(TransacaoDto.Tipo),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 14
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Categoria",
            DataPropertyName = nameof(TransacaoDto.CategoriaNome),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 20
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Conta",
            DataPropertyName = nameof(TransacaoDto.ContaNome),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 18
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Valor",
            DataPropertyName = nameof(TransacaoDto.Valor),
            DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 16
        });

        _grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            HeaderText = "Pago",
            DataPropertyName = nameof(TransacaoDto.Pago),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 10
        });

        _grid.SelectionChanged += (_, _) => PreencherFormularioComSelecao();

        return _grid;
    }

    private void PreencherFormularioComSelecao()
    {
        if (_grid.SelectedRows.Count == 0 ||
            _grid.SelectedRows[0].DataBoundItem is not TransacaoDto transacao)
        {
            return;
        }

        _transacaoSelecionadaId = transacao.Id;
        _descricaoTextBox.Text = transacao.Descricao;
        _tipoComboBox.SelectedItem = transacao.Tipo;
        AtualizarCategoriaComboBox();
        SelecionarCategoria(transacao.CategoriaId);
        SelecionarConta(transacao.ContaId);
        _valorInput.Value = transacao.Valor;
        _dataInput.Value = transacao.Data;
        _observacaoTextBox.Text = transacao.Observacao;
        _pagoCheckBox.Checked = transacao.Pago;
        _recorrenteCheckBox.Checked = transacao.Recorrente;
    }

    private void LimparFormulario()
    {
        _transacaoSelecionadaId = null;
        _descricaoTextBox.Clear();
        SelecionarPrimeiroItem(_tipoComboBox);
        AtualizarCategoriaComboBox();
        AtualizarContaComboBox();
        _valorInput.Value = 0;
        _dataInput.Value = DateTime.Today;
        _observacaoTextBox.Clear();
        _pagoCheckBox.Checked = true;
        _recorrenteCheckBox.Checked = false;
        _grid.ClearSelection();
    }

    private void AtualizarCategoriaComboBox()
    {
        if (_tipoComboBox.SelectedItem is not TipoTransacao tipoTransacao)
        {
            return;
        }

        var tipoCategoria = tipoTransacao == TipoTransacao.Receita
            ? TipoCategoria.Receita
            : TipoCategoria.Despesa;

        _categoriaComboBox.DataSource = _categorias
            .Where(categoria => categoria.Tipo == tipoCategoria && categoria.Ativa)
            .OrderBy(categoria => categoria.Nome)
            .ToList();
    }

    private void AtualizarContaComboBox()
    {
        _contaComboBox.DataSource = _contas
            .Where(conta => conta.Ativa)
            .OrderBy(conta => conta.Nome)
            .ToList();
    }

    private int ObterCategoriaSelecionadaId()
    {
        if (_categoriaComboBox.SelectedItem is CategoriaDto categoria)
        {
            return categoria.Id;
        }

        throw new InvalidOperationException("Selecione uma categoria.");
    }

    private int ObterContaSelecionadaId()
    {
        if (_contaComboBox.SelectedItem is ContaDto conta)
        {
            return conta.Id;
        }

        throw new InvalidOperationException("Selecione uma conta.");
    }

    private void SelecionarCategoria(int categoriaId)
    {
        foreach (var item in _categoriaComboBox.Items)
        {
            if (item is CategoriaDto categoria && categoria.Id == categoriaId)
            {
                _categoriaComboBox.SelectedItem = categoria;
                return;
            }
        }
    }

    private void SelecionarConta(int contaId)
    {
        foreach (var item in _contaComboBox.Items)
        {
            if (item is ContaDto conta && conta.Id == contaId)
            {
                _contaComboBox.SelectedItem = conta;
                return;
            }
        }
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
