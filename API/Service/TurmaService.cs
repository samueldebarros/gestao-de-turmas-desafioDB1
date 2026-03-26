using API.DTOs.TurmaDTOs;
using Common.Domains;
using Common.Enums;
using Common.Exceptions;
using Repository;

namespace API.Service;

public class TurmaService : ITurmaService
{
    private readonly ITurmaRepository _turmaRepository;

    public TurmaService(ITurmaRepository turmaRepository)
    {
        _turmaRepository = turmaRepository;
    }

    public async Task<List<Turma>> ObterTodasAsTurmasAsync()
    {
        var turmas = await _turmaRepository.ObterTodasAsTurmasAsync();
        return turmas;

    }
}
