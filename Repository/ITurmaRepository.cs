using Common.Domains;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public interface ITurmaRepository
{
    Task<Turma?> ObterTurmaComDetalhesAsync(int id);
    Task<List<Turma>> ObterTodasAsTurmasAsync(string? pesquisa = null, bool? ativo = null, OrdenacaoTurmaEnum? ordenacao = null);
    Task<Turma?> ObterPorIdAsync(int id);
    Task AdicionarTurmaAsync(Turma turma);
    Task EditarTurmaAsync(Turma turma);
    Task InativarTurmaAsync(int id);
    Task ReativarTurmaAsync(int id);
    Task<bool> ExisteTurmaAsync(string identificador, int anoLetivo, SerieEnum serie, int? ignorarId = null);
}