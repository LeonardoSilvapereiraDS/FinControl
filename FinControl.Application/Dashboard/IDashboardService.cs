namespace FinControl.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardResumoDto> ObterResumoAsync(
        int usuarioId,
        DateTime dataReferencia,
        CancellationToken cancellationToken = default);
}
