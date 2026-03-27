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

    public async Task AdicionarTurmaAsync(TurmaInputDTO turmaDTO)
    {
        var turma = new Turma()
        {
            Identificador = turmaDTO.Identificador,
            Serie = turmaDTO.Serie,
            Turno = turmaDTO.Turno,
            AnoLetivo = turmaDTO.AnoLetivo,
            Capacidade = turmaDTO.Capacidade,
            Ativo = true,
        };

        await _turmaRepository.AdicionarTurmaAsync(turma);
    }

    public async Task<List<Turma>> ObterTodasAsTurmasAsync()
    { 
        return await _turmaRepository.ObterTodasAsTurmasAsync();
    }

   
}
