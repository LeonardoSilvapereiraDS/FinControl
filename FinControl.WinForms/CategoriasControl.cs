using FinControl.Application.Categorias;
using FinControl.Domain.Enums;
using FinControl.Infrastructure.Persistence;

namespace FinControl.WinForms;

public sealed class CategoriasControl : UserControl
{
    private readonly ICategoriaService _categoriaService;
    private readonly BindingSource _bindingSource = new();
    private readonly DataGridView _grid = new();
    private readonly TextBox _nomeTextBox = new();
    private readonly ComboBox _tipoComboBox = new();
    private readonly CheckBox _incluirInativasCheckBox = new();
    private readonly Label _statusLabel = new();
    private int? _categoriaSelecionadaId;

    public event EventHandler? DadosAlterados;

    public CategoriasControl(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;

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
            var categorias = await _categoriaService.ListarAsync(
                BancoDadosInicializador.UsuarioPadraoId,
                _incluirInativasCheckBox.Checked);

            _bindingSource.DataSource = categorias;
            LimparFormulario();
            _statusLabel.Text = $"{categorias.Count} categorias";
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
            var request = new SalvarCategoriaRequest(
                _nomeTextBox.Text,
                (TipoCategoria)_tipoComboBox.SelectedItem!);

            if (_categoriaSelecionadaId is int categoriaId)
            {
                await _categoriaService.AtualizarAsync(
                    BancoDadosInicializador.UsuarioPadraoId,
                    categoriaId,
                    request);
            }
            else
            {
                await _categoriaService.CriarAsync(
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
        if (_categoriaSelecionadaId is not int categoriaId)
        {
            return;
        }

        try
        {
            await _categoriaService.DesativarAsync(BancoDadosInicializador.UsuarioPadraoId, categoriaId);
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
        if (_categoriaSelecionadaId is not int categoriaId)
        {
            return;
        }

        try
        {
            await _categoriaService.ReativarAsync(BancoDadosInicializador.UsuarioPadraoId, categoriaId);
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
            RowCount = 10,
            Padding = new Padding(0, 0, 18, 0),
            BackColor = Color.Transparent
        };

        for (var i = 0; i < painel.RowCount; i++)
        {
            painel.RowStyles.Add(new RowStyle(SizeType.Absolute, i is 0 ? 44 : 42));
        }

        _tipoComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _tipoComboBox.Items.AddRange(Enum.GetValues<TipoCategoria>().Cast<object>().ToArray());
        SelecionarPrimeiroItem(_tipoComboBox);

        var salvarButton = CriarBotao("Salvar");
        var novoButton = CriarBotao("Novo");
        var desativarButton = CriarBotao("Desativar");
        var reativarButton = CriarBotao("Reativar");

        salvarButton.Click += async (_, _) => await SalvarAsync();
        novoButton.Click += (_, _) => LimparFormulario();
        desativarButton.Click += async (_, _) => await DesativarAsync();
        reativarButton.Click += async (_, _) => await ReativarAsync();
        _incluirInativasCheckBox.CheckedChanged += async (_, _) => await CarregarAsync();

        painel.Controls.Add(CriarTitulo("Categoria"), 0, 0);
        painel.Controls.Add(CriarCampo("Nome", _nomeTextBox), 0, 1);
        painel.Controls.Add(CriarCampo("Tipo", _tipoComboBox), 0, 2);
        painel.Controls.Add(salvarButton, 0, 3);
        painel.Controls.Add(novoButton, 0, 4);
        painel.Controls.Add(desativarButton, 0, 5);
        painel.Controls.Add(reativarButton, 0, 6);
        painel.Controls.Add(_incluirInativasCheckBox, 0, 7);

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
            DataPropertyName = nameof(CategoriaDto.Nome),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 55
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Tipo",
            DataPropertyName = nameof(CategoriaDto.Tipo),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 25
        });

        _grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            HeaderText = "Ativa",
            DataPropertyName = nameof(CategoriaDto.Ativa),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 20
        });

        _grid.SelectionChanged += (_, _) => PreencherFormularioComSelecao();

        return _grid;
    }

    private void PreencherFormularioComSelecao()
    {
        if (_grid.SelectedRows.Count == 0 ||
            _grid.SelectedRows[0].DataBoundItem is not CategoriaDto categoria)
        {
            return;
        }

        _categoriaSelecionadaId = categoria.Id;
        _nomeTextBox.Text = categoria.Nome;
        _tipoComboBox.SelectedItem = categoria.Tipo;
    }

    private void LimparFormulario()
    {
        _categoriaSelecionadaId = null;
        _nomeTextBox.Clear();
        SelecionarPrimeiroItem(_tipoComboBox);
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
