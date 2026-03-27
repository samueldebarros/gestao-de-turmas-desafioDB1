using API.DTOs.TurmaDTOs;
using Common.Domains;
using Common.Enums;

namespace API.Service;

public interface ITurmaService
{
    Task<List<Turma>> ObterTodasAsTurmasAsync();
    Task AdicionarTurmaAsync(TurmaInputDTO turma);
}
