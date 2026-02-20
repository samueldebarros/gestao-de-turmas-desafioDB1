using Common.Domains;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public class AlunoRepository : IAlunoRepository
    {
        private readonly GestaoEscolarContext _context;

        public AlunoRepository(GestaoEscolarContext context)
        {
            _context = context;
        }

        public void Adicionar(Aluno aluno)
        {
            _context.Add(aluno);
            _context.SaveChanges();
        }

        public (List<Aluno> lista, int total) ObterTodosOsAluno(int pagina = 1, int tamanho = 10, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null)
        {
            var query = _context.Alunos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(pesquisa))
            {
                query = query.Where(a => a.Nome.Contains(pesquisa) || a.Cpf.Contains(pesquisa) || a.Matricula.Contains(pesquisa));
            }

            if (sexo.HasValue)
            {
                query = query.Where(a => a.Sexo == sexo.Value);
            }

            if (ativo.HasValue)
            {
                query = query.Where(a => a.Ativo == ativo.Value);
            }

            int total = query.Count();

            var lista = query.OrderBy(a => a.Nome)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .ToList();

            return (lista, total);
        }

        public void Excluir(int id)
        {
            var alunoExcluido = ObterPorId(id);

            if (alunoExcluido != null) 
            {
                _context.Remove(alunoExcluido);
                _context.SaveChanges();
            }
            
        }

        public Aluno ObterPorId(int id)
        {
            return _context.Alunos.FirstOrDefault(a => a.Id == id);
        }

        public void Alterar(Aluno aluno)
        {
            _context.Alunos.Update(aluno);
            _context.SaveChanges();
        }

        public bool ExistePeloCPF(string cpf)
        {
            return _context.Alunos.Any(a => a.Cpf == cpf);
        }
    }
}
