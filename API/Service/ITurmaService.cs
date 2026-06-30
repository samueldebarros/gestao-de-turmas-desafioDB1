using API.DTOs;
using API.DTOs.TurmaDTOs;
using Common.Domains;
using Common.Enums;

namespace API.Service;

public interface ITurmaService
{
    Task<List<Turma>> ObterTodasAsTurmasAsync();
    Task AdicionarTurmaAsync(TurmaInputDTO turma);
    Task<Turma> ObterTurmaPeloIdAsync(int id);
    Task EditarTurmaAsync(TurmaEditarDTO turmaDTO);
    Task<List<ListaTurmasDTO>> ObterTurmasSimplificadasAsync(string? pesquisa = null, OrdenacaoTurmaEnum? ordenacao = null);
    Task<ListaPaginada<ListaTurmasDTO>> ObterTurmasAsync(
    int pagina = 1, int tamanho = 12, string? pesquisa = null, int? anoLetivo = null,
    TurnoEnum? turno = null, bool? ativo = null, OrdenacaoTurmaEnum? ordenacao = null);
}
