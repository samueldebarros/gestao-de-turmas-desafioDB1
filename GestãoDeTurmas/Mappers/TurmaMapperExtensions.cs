using API.DTOs.TurmaDTOs;
using Common.Domains;
using GestãoDeTurmas.Models.Turma;

namespace GestãoDeTurmas.Mappers;

public static class TurmaMapperExtensions
{
    public static ListaTurmaViewModel ToListaViewModel(this Turma turma)
    {
        return new ListaTurmaViewModel()
        {
            TurmaId = turma.Id,
            Identificador = turma.Identificador,
            AnoLetivo = turma.AnoLetivo,
            Turno = turma.Turno,
            Capacidade = turma.Capacidade,
            Serie = turma.Serie,
            QuantidadeAlunos = turma.Enturmamentos.Count(),
            QuantidadeDisciplinas = turma.GradeCurricular.Count()
        };
    }

    public static TurmaInputDTO ToInputDTO(this TurmaInputViewModel model)
    {
        return new TurmaInputDTO()
        {
            Identificador = model.Identificador,
            Serie = model.Serie,
            Turno = model.Turno,
            AnoLetivo = model.AnoLetivo,
            Capacidade = model.Capacidade
        };
    }
}
