using API.DTOs.DashboardDTOs;

namespace API.Service;

public interface IDashboardService
{
    Task<DashboardDadosDTO> ObterDadosDashboard();

    //TODO (7) Revisar: assinaturas dos relatorios no servico de dashboard
    Task<List<PainelDemograficoTurmaDTO>> ObterPainelDemograficoPorTurmaAsync();
    Task<List<BalancoEvasaoSerieDTO>> ObterBalancoEvasaoPorSerieAsync();
}
