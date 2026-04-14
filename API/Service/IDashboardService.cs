using API.DTOs.DashboardDTOs;

namespace API.Service;

public interface IDashboardService
{
    Task<DashboardDadosDTO> ObterDadosDashboard();
}
