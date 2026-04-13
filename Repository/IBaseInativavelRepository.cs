using Common.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public interface IBaseInativavelRepository<T> : IBaseRepository<T> where T : class , IEntidadeInativavel
{

    Task<T?> ObterInativoPorIdAsync(int id);
    Task InativarAsync(int id);
    Task ReativarAsync(int id);
}
