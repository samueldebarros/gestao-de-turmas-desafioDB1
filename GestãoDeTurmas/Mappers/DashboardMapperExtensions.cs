using API.DTOs.DashboardDTOs;
using GestãoDeTurmas.Models.Dashboard;

namespace GestãoDeTurmas.Mappers;

public static class DashboardMapperExtensions
{
    public static DashboardDadosViewModel ToViewModel(this DashboardDadosDTO dto)
    {
        return new DashboardDadosViewModel()
        {
            TotalAlunos = dto.TotalAlunos,
            TotalDisciplinas = dto.TotalDisciplinas,
            TotalDocentes = dto.TotalDocentes,
            TotalTurmas = dto.TotalTurmas,
            AlunosInativos = dto.AlunosInativos,
            DocentesInativos = dto.DocentesInativos,
            DisciplinasInativos = dto.DisciplinasInativos
        };
    }
}
