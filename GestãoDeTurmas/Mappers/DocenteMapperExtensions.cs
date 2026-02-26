using API.DTOs.DocenteDTOs;
using GestãoDeTurmas.Models.Docente;

namespace GestãoDeTurmas.Mappers;

public static class DocenteMapperExtensions
{

    public static DocenteInputDTO ToDTO(this DocenteInputViewModel docente)
    {
        return new DocenteInputDTO
        {
            Nome = docente.Nome,
            Cpf = docente.Cpf,
            DataNascimento = docente.DataNascimento,
            Email = docente.Email,
            Especialidade = docente.Especialidade
        };
    }
}
