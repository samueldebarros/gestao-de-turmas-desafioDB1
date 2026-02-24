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

        public async Task AdicionarAsync(Aluno aluno)
        {
            await _context.AddAsync(aluno);
            _context.SaveChanges();
        }

        public async Task<(List<Aluno> lista, int total)> ObterTodosOsAlunoAsync(int pagina = 1, int tamanho = 10, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null)
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

            int total = await query.CountAsync();

            var lista = await query.OrderBy(a => a.Nome)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .ToListAsync();

            return (lista, total);
        }

        public async Task ExcluirAsync(int id) // fiz um soft delete ao inves de excluir do banco
        {
            await _context.Alunos
                .Where(a => a.Id == id)
                .ExecuteUpdateAsync(a => a.SetProperty(a => a.Ativo, false));
        }

        public async Task<Aluno> ObterPorIdAsync(int id)
        {
            return await _context.Alunos.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AlterarAsync(Aluno aluno)
        {
            _context.Alunos.Update(aluno);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistePeloCPFAsync(string cpf)
        {
            return await _context.Alunos.AnyAsync(a => a.Cpf == cpf);
        }
    }
}
