using Common.Domains;
using Repository.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public class DisciplinaRepository : IDisciplinaRepository
{
    public readonly GestaoEscolarContext _context;

    public DisciplinaRepository(GestaoEscolarContext context)
    {
        _context = context;
    }

    public async Task AdicionarDisciplinaAsync(Disciplina disciplina)
    {
        _context.Add(disciplina);
        await _context.SaveChangesAsync();

    }

    public Task EditarDisciplinaAsync()
    {
        throw new NotImplementedException();
    }

    public Task InativarDisciplinaAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Disciplina> ObterDisciplinaPorIdAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Disciplina> ObterInativoPorIdAsync()
    {
        throw new NotImplementedException();
    }

    public Task<(List<Disciplina>, int total)> ObterTodasAsDisciplinasAsync()
    {
        throw new NotImplementedException();
    }

    public Task ReativarDisciplinaAsync()
    {
        throw new NotImplementedException();
    }
}
