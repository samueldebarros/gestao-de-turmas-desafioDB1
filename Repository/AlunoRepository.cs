using Common.Domains;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using Repository.Context;

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
            _context.Add(aluno);

            await _context.SaveChangesAsync();
        }

        public async Task<(List<Aluno> lista, int total)> ObterTodosOsAlunoAsync(int pagina = 1, int tamanho = 10, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null)
        {
            var query = _context.Alunos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(pesquisa)) 
            {
                var pesquisaLimpaCpf = pesquisa.Replace(".", "").Replace("-", "");
                query = query.Where(a =>
                    a.Nome.Contains(pesquisa) ||
                    a.Cpf.Contains(pesquisaLimpaCpf) ||
                    a.Matricula.Contains(pesquisa));
            } 
         
            if (sexo.HasValue) query = query.Where(a => a.Sexo == sexo.Value);

            if (ativo.HasValue) query = query.Where(a => a.Ativo == ativo.Value);        

            int total = await query.CountAsync();

            var lista = await query.OrderBy(a => a.Nome)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .ToListAsync();

            return (lista, total);
        }

        public async Task InativarAsync(int id) 
        {
            await _context.Alunos
                .Where(a => a.Id == id)
                .ExecuteUpdateAsync(a => a.SetProperty(a => a.Ativo, false));
        }

        public async Task ReativarAsync(int id)
        {
            await _context.Alunos
                .Where(a => a.Id == id)
                .ExecuteUpdateAsync(a => a.SetProperty(a => a.Ativo, true));
        }

        public async Task<Aluno> ObterPorIdAsync(int id)
        {
            return await _context.Alunos.FirstOrDefaultAsync(a => a.Id == id && a.Ativo);
        }
        public async Task<Aluno> ObterInativoPorIdAsync(int id)
        {
            return await _context.Alunos.FirstOrDefaultAsync(a => a.Id == id && !a.Ativo);
        }

        public async Task AlterarAsync(Aluno aluno)
        {
            _context.Alunos.Update(aluno);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistePeloCpfAsync(string cpf)
        {
            return await _context.Alunos.AnyAsync(a => a.Cpf == cpf);
        }

        public async Task<bool> ExistePeloEmailAsync(string email, int? ignorarId = null)
        {
            var query = _context.Alunos.Where(a => a.Email == email);

            if(ignorarId.HasValue) 
                query = query.Where(a => a.Id != ignorarId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> ExisteMatriculaAsync(string matricula)
        {
            return await _context.Alunos.AnyAsync(d => d.Matricula == matricula);
        }
    }
}
