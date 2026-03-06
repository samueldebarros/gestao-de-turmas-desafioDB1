using Common.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public interface IDocenteRepository
{
    Task AdicionarDocenteAsync(Docente docente);
    Task<(List<Docente>, int total)> ObterTodosOsDocentesAsync(int pagina = 1, int tamanho = 5, string? pesquisa = null, bool? ativo = null);
    Task InativarDocenteAsync(int id);
    Task ReativarDocenteAsync(int id);
    Task<Docente> ObterPeloIdAsync(int id);
    Task<Docente> ObterInativoPeloIdAsync(int id);
    Task EditarDocenteAsync(Docente docente);
    Task<bool> ExistePeloCpfAsync(string cpf);
    Task<bool> ExistePeloEmailAsync(string email, int? ignorarId = null);
}
