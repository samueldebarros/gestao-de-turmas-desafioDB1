using Common.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public interface ITurmaRepository
{
    Task<List<Turma>> ListarAsync();
    Task<Turma?> ObterPorIdAsync(int id);
    Task AdicionarAsync(Turma turma);
    Task AtualizarAsync(Turma turma);
}