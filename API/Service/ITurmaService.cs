using API.DTOs.TurmaDTOs;
using Common.Domains;

namespace API.Service;

public interface ITurmaService
{
    Task<List<Turma>> ObterTodasAsTurmasAsync();
    Task<Turma> ObterPorIdAsync(int id);
    Task AdicionarTurmaAsync(TurmaInputDTO turmaDto);
    Task EditarTurmaAsync(EditarTurmaDTO turmaDto);
    Task InativarTurmaAsync(int id);
    Task ReativarTurmaAsync(int id);
}
