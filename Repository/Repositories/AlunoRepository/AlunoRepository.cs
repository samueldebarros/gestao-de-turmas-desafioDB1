using Common.Domains;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Repository.Repositories;
using Repository.Repositories.Base;
using System.Text.RegularExpressions;

namespace Repository.Repositories;

public class AlunoRepository : BaseInativavelRepository<Aluno>, IAlunoRepository
{
    public AlunoRepository(GestaoEscolarContext context) : base(context) { }

    private IQueryable<Aluno> AplicarOrdenacao(IQueryable<Aluno> query, OrdenacaoAlunoEnum? ordenacao = null, DirecaoOrdenacaoEnum? direcao = null)
    {
        bool isDesc = direcao == DirecaoOrdenacaoEnum.Desc;

        return ordenacao switch
        { 
            OrdenacaoAlunoEnum.Matricula => isDesc ? query.OrderByDescending(a => a.Matricula) : query.OrderBy(a => a.Matricula),
            OrdenacaoAlunoEnum.Nome => isDesc ? query.OrderByDescending(a => a.Nome) : query.OrderBy(a => a.Nome),
            OrdenacaoAlunoEnum.DataNascimento => isDesc ? query.OrderByDescending(a => a.DataNascimento) : query.OrderBy(a => a.DataNascimento),
            _ => query.OrderBy(a => a.Nome)
        };
    }

    public async Task<(List<Aluno> lista, int total)> ObterTodosOsAlunoAsync(int pagina = 1, int tamanho = 10, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null,
        OrdenacaoAlunoEnum? ordenacao = null, DirecaoOrdenacaoEnum? direcao = null)
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

        // SELECT * FROM Alunos 
        // WHERE (Nome LIKE '%14150309957%' OR Cpf = '14150309957' OR Matricula = '14150309957' ) 
        // AND Ativo = 1 AND Sexo = 'Masculino' 
        // ORDER BY Nome ASC;
        // [LIKE para pesquisa mais modular mas menos performático e = para pesquisas exatas]
        // Para ter um resultado parecido, eu teria que trocar o Contains de CPF e Matriucla pelo == ou usar o .startWith para simular o LIKE '141%'


        int total = await query.CountAsync();

        query = AplicarOrdenacao(query, ordenacao, direcao);

        var lista = await query.Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        //OFFSET 5 ROWS 
        //FETCH NEXT 5 ROWS ONLY;

        return (lista, total);
    }

    public async Task<List<Aluno>> ObterAlunosDisponiveisParaTurmaAsync(int turmaId)
    {

        return await _dbSet
            .AsNoTracking()
            .Where(a => a.Ativo && !_context.Enturmamentos.Any(e => e.TurmaId == turmaId && e.AlunoId == a.Id))
            .OrderBy(a => a.Nome)
            .ToListAsync();

        /*  
         *  SELECT * 
         *  FROM Alunos AS a
         *  WHERE a.Ativo = 1
         *  AND NOT EXISTS (
         *      SELECT 1
         *      FROM Enturmamentos AS e
         *      JOIN Turmas AS t ON e.TurmaId = t.Id
         *      WHERE e.AlunoId = a.Id AND e.TurmaId = turmaId
         *  )
        */
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

    public async Task<List<Aluno>> ObterAlunosDisponiveisParaTurmaSQL(int turmaId)
    {
        return await _dbSet
            .FromSqlInterpolated($@"
                SELECT a.Id, a.Matricula, a.Nome, a.Cpf, a.DataNascimento, a.Sexo, a.Email, a.Ativo 
                FROM Alunos AS a
                WHERE a.Ativo = 1
                AND NOT EXISTS (
                   SELECT 1
                   FROM Enturmamentos AS e
                   JOIN Turmas AS t ON e.TurmaId = t.Id
                   WHERE e.AlunoId = a.Id AND e.TurmaId = turmaId
                )
                ORDER BY a.Nome ASC
            ")
            .AsNoTracking()
            .ToListAsync();
    }
}
