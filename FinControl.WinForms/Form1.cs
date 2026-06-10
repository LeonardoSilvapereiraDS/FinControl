namespace FinControl.WinForms;

public partial class Form1 : Form
{
    private readonly DashboardControl _dashboardControl;

    public Form1(
        DashboardControl dashboardControl,
        CategoriasControl categoriasControl,
        ContasControl contasControl,
        TransacoesControl transacoesControl)
    {
        _dashboardControl = dashboardControl;

        InitializeComponent();
        ConfigurarTela(dashboardControl, categoriasControl, contasControl, transacoesControl);

        categoriasControl.DadosAlterados += async (_, _) => await _dashboardControl.AtualizarAsync();
        contasControl.DadosAlterados += async (_, _) => await _dashboardControl.AtualizarAsync();
        transacoesControl.DadosAlterados += async (_, _) => await _dashboardControl.AtualizarAsync();
    }

    private void ConfigurarTela(
        DashboardControl dashboardControl,
        CategoriasControl categoriasControl,
        ContasControl contasControl,
        TransacoesControl transacoesControl)
    {
        SuspendLayout();

        BackColor = Color.FromArgb(246, 248, 250);
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        Text = "FinControl";
        MinimumSize = new Size(1040, 680);

        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(28),
            BackColor = BackColor
        };

        container.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var cabecalho = CriarCabecalho();
        var abas = CriarAbas(dashboardControl, categoriasControl, contasControl, transacoesControl);

        container.Controls.Add(cabecalho, 0, 0);
        container.Controls.Add(abas, 0, 1);

        Controls.Add(container);

        ResumeLayout();
    }

    private static Control CriarCabecalho()
    {
        var painel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };

        painel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        painel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

        painel.Controls.Add(new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Color.FromArgb(31, 35, 40),
            Text = "FinControl"
        }, 0, 0);

        painel.Controls.Add(new Label
        {
            AutoSize = true,
            ForeColor = Color.FromArgb(88, 96, 105),
            Text = "Controle financeiro pessoal"
        }, 0, 1);

        return painel;
    }

    private static Control CriarAbas(
        DashboardControl dashboardControl,
        CategoriasControl categoriasControl,
        ContasControl contasControl,
        TransacoesControl transacoesControl)
    {
        var abas = new TabControl
        {
            Dock = DockStyle.Fill,
            Appearance = TabAppearance.Normal
        };

        abas.TabPages.Add(CriarAba("Dashboard", dashboardControl));
        abas.TabPages.Add(CriarAba("Transacoes", transacoesControl));
        abas.TabPages.Add(CriarAba("Categorias", categoriasControl));
        abas.TabPages.Add(CriarAba("Contas", contasControl));

        return abas;
    }

    private static TabPage CriarAba(string titulo, Control conteudo)
    {
        conteudo.Dock = DockStyle.Fill;

        var aba = new TabPage(titulo)
        {
            BackColor = Color.FromArgb(246, 248, 250),
            Padding = new Padding(12)
        };

        aba.Controls.Add(conteudo);

        return aba;
    }
}
