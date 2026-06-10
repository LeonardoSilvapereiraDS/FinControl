using FinControl.Application.Abstractions.Persistence;
using FinControl.Domain.Entities;
using FinControl.Domain.Enums;

namespace FinControl.Application.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly IRepositorio<Conta> _contas;
    private readonly IRepositorio<MetaFinanceira> _metas;
    private readonly IRepositorio<Transacao> _transacoes;

    public DashboardService(
        IRepositorio<Conta> contas,
        IRepositorio<MetaFinanceira> metas,
        IRepositorio<Transacao> transacoes)
    {
        _contas = contas;
        _metas = metas;
        _transacoes = transacoes;
    }

    public async Task<DashboardResumoDto> ObterResumoAsync(
        int usuarioId,
        DateTime dataReferencia,
        CancellationToken cancellationToken = default)
    {
        var transacoes = await _transacoes.ListarAsync(cancellationToken);
        var contas = await _contas.ListarAsync(cancellationToken);
        var metas = await _metas.ListarAsync(cancellationToken);

        var transacoesDoUsuario = transacoes
            .Where(transacao => transacao.UsuarioId == usuarioId && transacao.Pago)
            .ToList();

        var transacoesDoMes = transacoesDoUsuario
            .Where(transacao =>
                transacao.Data.Month == dataReferencia.Month &&
                transacao.Data.Year == dataReferencia.Year)
            .ToList();

        var receitasMes = transacoesDoMes
            .Where(transacao => transacao.Tipo == TipoTransacao.Receita)
            .Sum(transacao => transacao.Valor);

        var despesasMes = transacoesDoMes
            .Where(transacao => transacao.Tipo == TipoTransacao.Despesa)
            .Sum(transacao => transacao.Valor);

        var saldoInicial = contas
            .Where(conta => conta.UsuarioId == usuarioId && conta.Ativa)
            .Sum(conta => conta.SaldoInicial);

        var saldoAtual = saldoInicial + transacoesDoUsuario.Sum(transacao => transacao.ValorComSinal);

        var contasAtivas = contas.Count(conta => conta.UsuarioId == usuarioId && conta.Ativa);
        var metasEmAndamento = metas.Count(meta => meta.UsuarioId == usuarioId && !meta.Concluida);

        return new DashboardResumoDto(
            receitasMes,
            despesasMes,
            saldoAtual,
            transacoesDoMes.Count,
            contasAtivas,
            metasEmAndamento);
    }
}
