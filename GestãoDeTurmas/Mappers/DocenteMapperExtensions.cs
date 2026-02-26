using API.DTOs.DocenteDTOs;
using Common.Domains;
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

    public static DocenteListaViewModel ToListaViewModel(this Docente docente)
    {
        return new DocenteListaViewModel
        {
            Nome = docente.Nome,
            Especialidade = docente.Especialidade,
            Cpf = docente.Cpf,
            Email = docente.Email,
            Ativo = docente.Ativo,
            DataNascimento = docente.DataNascimento,
        };
    }
}
