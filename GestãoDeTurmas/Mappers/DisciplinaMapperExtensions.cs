using API.DTOs.DisciplinaDTOs;
using Common.Domains;
using GestãoDeTurmas.Models.Disciplina;

namespace GestãoDeTurmas.Mappers;

public static class DisciplinaMapperExtensions
{
    public static DisciplinaInputDTO ToInputDTO(this DisciplinaInputViewModel model)
    {
        return new DisciplinaInputDTO
        {
            Nome = model.Nome,
            CargaHoraria = model.CargaHoraria,
            Ementa = model.Ementa
        };
    }

    public static DisciplinaListaViewModel ToListaViewModel(this Disciplina disciplina)
    {
        return new DisciplinaListaViewModel
        {
            Id = disciplina.Id,
            Nome = disciplina.Nome,
            CargaHoraria = disciplina.CargaHoraria,
            Ementa = disciplina.Ementa,
            Ativo = disciplina.Ativo
        };
    }

    public static DisciplinaEditarViewModel ToEditarViewModel(this Disciplina disciplina)
    {
        return new DisciplinaEditarViewModel
        {
            Id = disciplina.Id,
            Nome = disciplina.Nome,
            CargaHoraria = disciplina.CargaHoraria,
            Ementa = disciplina.Ementa
        };
    }

    public static EditarDisciplinaDTO ToEditarDTO(this DisciplinaEditarViewModel model)
    {
        return new EditarDisciplinaDTO
        {
            Id = model.Id,
            Nome = model.Nome,
            CargaHoraria = model.CargaHoraria,
            Ementa = model.Ementa
        };
    }

}
