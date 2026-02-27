using Common.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public interface IDocenteRepository
{
    Task AdicionarDocenteAsync(Docente docente);
    Task<List<Docente>> ObterTodosOsDocentesAsync();
    Task InativarDocenteAsync(int id);
    Task ReativarDocenteAsync(int id);
    Task<Docente> ObterPeloIdAsync(int id);
    Task EditarDocenteAsync(Docente docente);
}
