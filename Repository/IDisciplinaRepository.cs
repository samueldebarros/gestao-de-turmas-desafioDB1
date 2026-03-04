using Common.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public interface IDisciplinaRepository
{
    Task<Disciplina> ObterDisciplinaPorIdAsync();
    Task<Disciplina> ObterInativoPorIdAsync();
    Task<(List<Disciplina>, int total)> ObterTodasAsDisciplinasAsync();
    Task AdicionarDisciplinaAsync();
    Task EditarDisciplinaAsync();
    Task InativarDisciplinaAsync();
    Task ReativarDisciplinaAsync();
    
    
}
