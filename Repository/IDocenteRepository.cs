using Common.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public interface IDocenteRepository
{
    Task AdicionarDocenteAsync(Docente docente);
    Task<List<Docente>> ObterTodosOsDocentesAsync();
}
