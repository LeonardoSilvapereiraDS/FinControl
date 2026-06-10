using System.Globalization;
using FinControl.Application.Dashboard;
using FinControl.Infrastructure.Persistence;

namespace FinControl.WinForms;

public sealed class DashboardControl : UserControl
{
    private readonly IDashboardService _dashboardService;
    private readonly CultureInfo _culturaMoeda = CultureInfo.GetCultureInfo("pt-BR");
    private readonly Label _saldoAtualValor = CriarValorCard();
    private readonly Label _receitasMesValor = CriarValorCard();
    private readonly Label _despesasMesValor = CriarValorCard();
    private readonly Label _transacoesMesValor = CriarValorCard();
    private readonly Label _contasAtivasValor = CriarValorCard();
    private readonly Label _metasValor = CriarValorCard();
    private readonly Label _statusLabel = CriarStatusLabel("Carregando dados...");

    public DashboardControl(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;

        ConfigurarTela();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        await AtualizarAsync();
    }

    public async Task AtualizarAsync()
    {
        try
        {
            var resumo = await _dashboardService.ObterResumoAsync(
                BancoDadosInicializador.UsuarioPadraoId,
                DateTime.Today);

            _saldoAtualValor.Text = resumo.SaldoAtual.ToString("C", _culturaMoeda);
            _receitasMesValor.Text = resumo.ReceitasMes.ToString("C", _culturaMoeda);
            _despesasMesValor.Text = resumo.DespesasMes.ToString("C", _culturaMoeda);
            _transacoesMesValor.Text = resumo.TotalTransacoesMes.ToString(_culturaMoeda);
            _contasAtivasValor.Text = resumo.ContasAtivas.ToString(_culturaMoeda);
            _metasValor.Text = resumo.MetasEmAndamento.ToString(_culturaMoeda);
            _statusLabel.Text = $"Atualizado em {DateTime.Now:dd/MM/yyyy HH:mm}";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Nao foi possivel carregar o dashboard.";
            MessageBox.Show(
                ex.Message,
                "FinControl",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ConfigurarTela()
    {
        BackColor = Color.FromArgb(246, 248, 250);

        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = BackColor
        };

        container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        container.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        container.Controls.Add(CriarGridCards(), 0, 0);
        container.Controls.Add(_statusLabel, 0, 1);

        Controls.Add(container);
    }

    private Control CriarGridCards()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            RowCount = 2,
            AutoSize = true,
            BackColor = Color.Transparent
        };

        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));

        grid.Controls.Add(CriarCard("Saldo atual", _saldoAtualValor), 0, 0);
        grid.Controls.Add(CriarCard("Receitas do mes", _receitasMesValor), 1, 0);
        grid.Controls.Add(CriarCard("Despesas do mes", _despesasMesValor), 2, 0);
        grid.Controls.Add(CriarCard("Transacoes do mes", _transacoesMesValor), 0, 1);
        grid.Controls.Add(CriarCard("Contas ativas", _contasAtivasValor), 1, 1);
        grid.Controls.Add(CriarCard("Metas em andamento", _metasValor), 2, 1);

        return grid;
    }

    private static Control CriarCard(string titulo, Label valor)
    {
        var card = new Panel
        {
            BackColor = Color.White,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 16, 16),
            Padding = new Padding(18),
            BorderStyle = BorderStyle.FixedSingle
        };

        var tituloLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = Color.FromArgb(88, 96, 105),
            Location = new Point(18, 18),
            Text = titulo
        };

        valor.Location = new Point(18, 52);

        card.Controls.Add(tituloLabel);
        card.Controls.Add(valor);

        return card;
    }

    private static Label CriarValorCard()
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Color.FromArgb(31, 35, 40),
            Text = "0"
        };
    }

    private static Label CriarStatusLabel(string texto)
    {
        return new Label
        {
            AutoSize = true,
            ForeColor = Color.FromArgb(88, 96, 105),
            Text = texto
        };
    }
}
