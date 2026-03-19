using API.DTOs.TurmaDTOs;
using Common.Domains;
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

    private async Task<Turma> ObterTurmaOuLancarErroAsync(int id)
    {
        var turma = await _turmaRepository.ObterPorIdAsync(id);
        if (turma == null)
            throw new EntidadeNaoEncontradaException("A turma não foi encontrada.");
        return turma;
    }
    public async Task<List<Turma>> ObterTodasAsTurmasAsync()
    {
        return await _turmaRepository.ObterTodasAsTurmasAsync();
    }
    public async Task<Turma> ObterPorIdAsync(int id)
    {
        return await ObterTurmaOuLancarErroAsync(id);
    }

    public async Task AdicionarTurmaAsync(TurmaInputDTO turmaDto)
    {
        Turma novaTurma = new Turma
        {
            AnoLetivo = turmaDto.AnoLetivo,
            Identificador = turmaDto.Identificador.ToUpper().Trim(),
            Serie = turmaDto.Serie,
            VagasMaximas = turmaDto.VagasMaximas,
            Ativo = true
        };

        await _turmaRepository.AdicionarTurmaAsync(novaTurma);
    }

    public async Task EditarTurmaAsync(EditarTurmaDTO turmaDto)
    {
        var turma = await ObterTurmaOuLancarErroAsync(turmaDto.Id);

        turma.AnoLetivo = turmaDto.AnoLetivo;
        turma.Identificador = turmaDto.Identificador.ToUpper().Trim();
        turma.Serie = turmaDto.Serie;
        turma.VagasMaximas = turmaDto.VagasMaximas;

        await _turmaRepository.EditarTurmaAsync(turma);
    }

    public async Task InativarTurmaAsync(int id)
    {
        await ObterTurmaOuLancarErroAsync(id);
        await _turmaRepository.InativarTurmaAsync(id);
    }

    public async Task ReativarTurmaAsync(int id)
    {
        await ObterTurmaOuLancarErroAsync(id);
        await _turmaRepository.ReativarTurmaAsync(id);
    }
}
