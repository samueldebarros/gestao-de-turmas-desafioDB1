using API.DTOs.AlunoDTOs;
using Common.Domains;
using GestãoDeTurmas.Models.Aluno;

namespace GestãoDeTurmas.Mappers
{
    public static class AlunoMapperExtensions
    {
        public static AlunoListaViewModel ToListaViewModel(this Aluno a)
        {
            return new AlunoListaViewModel
            {
                Id = a.Id,
                Matricula = a.Matricula,
                NomeExibicao = a.Nome,
                Cpf = a.Cpf,
                Email = a.Email,
                DataNascimento = a.DataNascimento,
                Ativo = a.Ativo,
                Sexo = a.Sexo
            };
        }

        public static AlunoInputDTO ToDTO(this AlunoInputViewModel aluno)
        {
            return new AlunoInputDTO
            {
                Nome = aluno.Nome,
                Cpf = aluno.Cpf,
                Email = aluno.Email,
                DataNascimento = aluno.DataNascimento.GetValueOrDefault(),
                Sexo = aluno.Sexo
            };
        }

        public static AlunoEditarViewModel ToEditarViewModel(this Aluno aluno)
        {
            return new AlunoEditarViewModel
            {
                Id = aluno.Id,
                Nome = aluno.Nome,
                Email = aluno.Email,
                DataNascimento = aluno.DataNascimento,
                Sexo = aluno.Sexo
            };
        }

        public static AlterarAlunoDTO ToAlterarDTO(this AlunoEditarViewModel model)
        {
            return new AlterarAlunoDTO
            {
                Id = model.Id,
                Nome = model.Nome,
                Email = model.Email,
                DataNascimento = model.DataNascimento,
                Sexo = model.Sexo
            };

        }
    }
}
