using Common.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public interface ITurmaRepository
{
    Task<List<Turma>> ObterTodasAsTurmasAsync();
    Task<Turma?> ObterPorIdAsync(int id);
    Task AdicionarTurmaAsync(Turma turma);
    Task EditarTurmaAsync(Turma turma);
    Task InativarTurmaAsync(int id);
    Task ReativarTurmaAsync(int id);
}