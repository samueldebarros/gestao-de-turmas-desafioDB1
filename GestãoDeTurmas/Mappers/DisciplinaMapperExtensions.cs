using API.DTOs.DisciplinaDTOs;
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

}
