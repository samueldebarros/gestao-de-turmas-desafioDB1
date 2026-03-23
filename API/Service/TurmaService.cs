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

    private async Task<Turma> ObterTurmaOuLancarErroAsync(int id)
    {
        var turma = await _turmaRepository.ObterPorIdAsync(id);
        if (turma == null)
            throw new EntidadeNaoEncontradaException("A turma não foi encontrada.");
        return turma;
    }
    public async Task<List<Turma>> ObterTodasAsTurmasAsync(string? pesquisa = null, bool? ativo = null, OrdenacaoTurmaEnum? ordenacao = null)
    {
        return await _turmaRepository.ObterTodasAsTurmasAsync(pesquisa,ativo,ordenacao);
    }
    public async Task<Turma> ObterPorIdAsync(int id)
    {
        return await ObterTurmaOuLancarErroAsync(id);
    }

    public async Task AdicionarTurmaAsync(TurmaInputDTO turmaDto)
    {
        if (await _turmaRepository.ExisteTurmaAsync(turmaDto.Identificador.ToUpper().Trim(), turmaDto.AnoLetivo, turmaDto.Serie))
            throw new RegraDeNegocioException("Já existe uma turma com esse identificador, ano letivo e série.");

        Turma novaTurma = new Turma
        {
            AnoLetivo = turmaDto.AnoLetivo,
            Identificador = turmaDto.Identificador.ToUpper().Trim(),
            Serie = turmaDto.Serie,
            Turno = turmaDto.Turno,
            Capacidade = turmaDto.Capacidade,
            Ativo = true
        };

        await _turmaRepository.AdicionarTurmaAsync(novaTurma);
    }

    public async Task EditarTurmaAsync(EditarTurmaDTO turmaDto)
    {
        var turma = await ObterTurmaOuLancarErroAsync(turmaDto.Id);

        if (await _turmaRepository.ExisteTurmaAsync(turmaDto.Identificador.ToUpper().Trim(), turmaDto.AnoLetivo, turmaDto.Serie, turmaDto.Id))
            throw new RegraDeNegocioException("Já existe uma turma com esse identificador, ano letivo e série.");

        turma.AnoLetivo = turmaDto.AnoLetivo;
        turma.Identificador = turmaDto.Identificador.ToUpper().Trim();
        turma.Serie = turmaDto.Serie;
        turma.Turno = turmaDto.Turno;
        turma.Capacidade = turmaDto.Capacidade;

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
