using API.DTOs.TurmaDTOs;
using Common.Domains;
using GestãoDeTurmas.Models.Turma;

namespace GestãoDeTurmas.Mappers;

public static class TurmaMapperExtensions
{
    public static TurmaListaViewModel ToListaViewModel(this Turma turma)
    {
        return new TurmaListaViewModel
        {
            Id = turma.Id,
            Identificador = turma.Identificador,
            AnoLetivo = turma.AnoLetivo,
            Ativo = turma.Ativo,
            Serie = turma.Serie,
            Turno = turma.Turno,
            TotalAlunos = turma.Enturmamentos?.Count ?? 0,
            TotalDisciplinas = turma.GradeCurricular?.Count ?? 0,
            Capacidade = turma.Capacidade
        };
    }

    public static TurmaInputDTO ToInputDTO(this TurmaInputViewModel model)
    {
        return new TurmaInputDTO
        {
            Identificador = model.Identificador,
            AnoLetivo = model.AnoLetivo,
            Serie = model.Serie,
            Turno = model.Turno,
            Capacidade = model.Capacidade
        };
    }

    public static TurmaEditarViewModel ToEditarViewModel(this Turma turma)
    {
        return new TurmaEditarViewModel
        {
            Id = turma.Id,
            Identificador = turma.Identificador,
            AnoLetivo = turma.AnoLetivo,
            Serie = turma.Serie,
            Turno = turma.Turno,
            Capacidade = turma.Capacidade,
        };
    }

    public static EditarTurmaDTO ToEditarDTO(this TurmaEditarViewModel turma)
    {
        return new EditarTurmaDTO
        {
            Id = turma.Id,
            Identificador = turma.Identificador,
            AnoLetivo = turma.AnoLetivo,
            Serie = turma.Serie,
            Turno = turma.Turno,
            Capacidade = turma.Capacidade,
        };
    }


}
