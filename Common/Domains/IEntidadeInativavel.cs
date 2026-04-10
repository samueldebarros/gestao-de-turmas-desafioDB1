using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domains;

public interface IEntidadeInativavel : IEntidade
{
    bool Ativo { get; set; }
}
