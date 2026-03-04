using Common.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public interface IDisciplinaRepository
{
    Task<Disciplina> ObterDisciplinaPorIdAsync();
    Task<Disciplina> ObterInativoPorIdAsync();
    Task<List<Disciplina>> ObterTodasAsDisciplinasAsync();
    Task AdicionarDisciplinaAsync(Disciplina disciplina);
    Task EditarDisciplinaAsync();
    Task InativarDisciplinaAsync();
    Task ReativarDisciplinaAsync();
    
    
}
