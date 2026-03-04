using Common.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public class DisciplinaRepository : IDisciplinaRepository
{
    public Task AdicionarDisciplinaAsync()
    {
        throw new NotImplementedException();
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
