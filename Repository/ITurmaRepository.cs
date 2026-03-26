using Common.Domains;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public interface ITurmaRepository
{
    public Task<List<Turma>> ObterTodasAsTurmasAsync();
}