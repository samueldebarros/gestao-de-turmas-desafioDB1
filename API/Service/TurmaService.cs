using API.DTOs.TurmaDTOs;
using Common.Domains;
using Common.Enums;
using Common.Exceptions;
using Repository.Repositories.TurmaRepository;

namespace API.Service;

public class TurmaService : ITurmaService
{
    private readonly ITurmaRepository _turmaRepository;

    public TurmaService(ITurmaRepository turmaRepository)
    {
        _turmaRepository = turmaRepository;
    }

    private async Task ValidarTurma(string identificador, SerieEnum serie, int anoLetivo, int? ignorarId = null)
    {
        if (await _turmaRepository.ValidarPelosIdentificadores(identificador, serie, anoLetivo, ignorarId))
            throw new RegraDeNegocioException("Já existe uma turma com essa combinação de Identificador, Série e Ano letivo");
    }

    public async Task AdicionarTurmaAsync(TurmaInputDTO turmaDTO)
    {
        await ValidarTurma(turmaDTO.Identificador, turmaDTO.Serie, turmaDTO.AnoLetivo);

        var turma = new Turma()
        {
            Identificador = turmaDTO.Identificador,
            Serie = turmaDTO.Serie,
            Turno = turmaDTO.Turno,
            AnoLetivo = turmaDTO.AnoLetivo,
            Capacidade = turmaDTO.Capacidade,
            Ativo = true,
        };

        await _turmaRepository.AdicionarAsync(turma);
    }

    public async Task EditarTurmaAsync(TurmaEditarDTO turmaDTO)
    {
        var turma = await _turmaRepository.ObterPorIdAsync(turmaDTO.Id);

        if (turma == null)
            throw new EntidadeNaoEncontradaException("Turma não encontrada.");

        await ValidarTurma(turmaDTO.Identificador, turmaDTO.Serie, turmaDTO.AnoLetivo, turmaDTO.Id);

        turma.Turno = turmaDTO.Turno;
        turma.Capacidade = turmaDTO.Capacidade;
        turma.Serie = turmaDTO.Serie;
        turma.AnoLetivo = turmaDTO.AnoLetivo;
        turma.Identificador = turmaDTO.Identificador;

        await _turmaRepository.EditarAsync(turma);
    }

    public async Task<List<Turma>> ObterTodasAsTurmasAsync()
    {
        return await _turmaRepository.ObterTodasAsTurmasAsync();
    }

    public async Task<List<ListaTurmasDTO>> ObterTurmasSimplificadasAsync(string? pesquisa = null, OrdenacaoTurmaEnum? ordenacao = null)
    {
        var turmas = await _turmaRepository.ObterTurmasSimplificadasAsync(pesquisa, ordenacao);

        var turmasDTO = turmas.Select(t => new ListaTurmasDTO
        {
            TurmaId = t.Id,
            Identificador = t.Identificador,
            Turno = t.Turno,
            AnoLetivo = t.AnoLetivo,
            Capacidade = t.Capacidade,
            QuantidadeAlunos = t.QuantidadeAlunos,
            QuantidadeDisciplinas = t.QuantidadeDisciplinas,
            Serie = t.Serie
        }).ToList();

        return turmasDTO;
    }

    public async Task<Turma> ObterTurmaPeloIdAsync(int id)
    {
        return await _turmaRepository.ObterPorIdAsync(id);
    }
}
