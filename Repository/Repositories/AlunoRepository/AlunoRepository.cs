using Common.Domains;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Repository.Repositories;
using Repository.Repositories.Base;

namespace Repository.Repositories;

public class AlunoRepository : BaseInativavelRepository<Aluno>, IAlunoRepository
{
    public AlunoRepository(GestaoEscolarContext context) : base(context) { }

    private IQueryable<Aluno> OrdenarLista(IQueryable<Aluno> query,string? ordenacao = null, DirecaoOrdenacaoEnum? direcao = null)
    {
        bool isDesc = direcao == DirecaoOrdenacaoEnum.Desc;

        return ordenacao switch
        { 
            "Matricula" => isDesc ? query.OrderByDescending(a => a.Matricula) : query.OrderBy(a => a.Matricula),
            "Nome" => isDesc ? query.OrderByDescending(a => a.Nome) : query.OrderBy(a => a.Nome),
            "DataNascimento" => isDesc ? query.OrderByDescending(a => a.DataNascimento) : query.OrderBy(a => a.DataNascimento),
            _ => query.OrderBy(a => a.Nome)
        };
    }

    public async Task<(List<Aluno> lista, int total)> ObterTodosOsAlunoAsync(int pagina = 1, int tamanho = 10, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null,
        string? ordenacao = null, DirecaoOrdenacaoEnum? direcao = null)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

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

        query = OrdenarLista(query, ordenacao, direcao);

        var lista = await query.Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return (lista, total);
    }

    public async Task<List<Aluno>> ObterAlunosDisponiveisParaTurmaAsync(int turmaId)
    {
        var alunosMatriculados = _context.Enturmamentos
            .Where(e => e.TurmaId == turmaId)
            .Select(e => e.AlunoId);

        return await _dbSet
            .AsNoTracking()
            .Where(a => a.Ativo && !alunosMatriculados.Contains(a.Id))
            .OrderBy(a => a.Nome)
            .ToListAsync();
    }

    public async Task<bool> ExistePeloCpfAsync(string cpf)
    {
        return await _dbSet.AnyAsync(a => a.Cpf == cpf);
    }

    public async Task<bool> ExistePeloEmailAsync(string email, int? ignorarId = null)
    {
        var query = _dbSet.Where(a => a.Email == email);

        if(ignorarId.HasValue) 
            query = query.Where(a => a.Id != ignorarId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> ExisteMatriculaAsync(string matricula)
    {
        return await _dbSet.AnyAsync(d => d.Matricula == matricula);
    }
}
