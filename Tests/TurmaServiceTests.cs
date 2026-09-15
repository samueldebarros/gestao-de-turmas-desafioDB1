namespace Tests;

using API.DTOs.TurmaDTOs;
using API.Service;
using Common.Domains;
using Common.Enums;
using Common.Exceptions;
using FluentAssertions;
using Moq;
using Repository.Repositories.DocenteRepository;
using Repository.Repositories.TurmaRepository;

public class TurmaServiceTests
{
    private readonly Mock<ITurmaRepository> _turmaRepositoryMock = new();
    private readonly Mock<IDocenteRepository> _docenteRepositoryMock = new();

    private TurmaService CriarService() =>
        new TurmaService(_turmaRepositoryMock.Object, _docenteRepositoryMock.Object);

    private static Turma CriarTurma(int id = 1, int capacidade = 30) => new Turma
    {
        Id = id,
        Identificador = "A",
        Serie = SerieEnum.PrimeiroAno,
        AnoLetivo = 2026,
        Turno = TurnoEnum.Matutino,
        Capacidade = capacidade,
        Ativo = true
    };

    private static TurmaEditarDTO CriarDtoEdicao(int id = 1, int capacidade = 30) => new TurmaEditarDTO
    {
        Id = id,
        Identificador = "A",
        Serie = SerieEnum.PrimeiroAno,
        AnoLetivo = 2026,
        Turno = TurnoEnum.Matutino,
        Capacidade = capacidade
    };

    [Fact]
    public async Task InativarTurma_ComAlunoAtivo_DeveLancarRegraDeNegocioExceptionENaoInativar()
    {
        _turmaRepositoryMock.Setup(repo => repo.ExisteAsync(1)).ReturnsAsync(true);
        _turmaRepositoryMock.Setup(repo => repo.ContarAlunosAtivosAsync(1)).ReturnsAsync(1);

        var turmaService = CriarService();

        Func<Task> acao = async () => await turmaService.InativarTurmaAsync(1);

        await acao.Should().ThrowAsync<RegraDeNegocioException>()
            .WithMessage("Não é possível inativar uma turma com alunos ativos. A turma possui 1 aluno(s) ativo(s).");

        _turmaRepositoryMock.Verify(repo => repo.InativarAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task InativarTurma_SemAlunoAtivo_DeveInativar()
    {
        _turmaRepositoryMock.Setup(repo => repo.ExisteAsync(1)).ReturnsAsync(true);
        _turmaRepositoryMock.Setup(repo => repo.ContarAlunosAtivosAsync(1)).ReturnsAsync(0);

        var turmaService = CriarService();

        await turmaService.InativarTurmaAsync(1);

        _turmaRepositoryMock.Verify(repo => repo.InativarAsync(1), Times.Once);
    }

    [Fact]
    public async Task InativarTurma_ComIdInexistente_DeveLancarEntidadeNaoEncontradaException()
    {
        _turmaRepositoryMock.Setup(repo => repo.ExisteAsync(999)).ReturnsAsync(false);

        var turmaService = CriarService();

        Func<Task> acao = async () => await turmaService.InativarTurmaAsync(999);

        await acao.Should().ThrowAsync<EntidadeNaoEncontradaException>()
            .WithMessage("Turma não encontrada.");

        _turmaRepositoryMock.Verify(repo => repo.InativarAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ReativarTurma_QueNaoEstaInativa_DeveLancarEntidadeNaoEncontradaException()
    {
        _turmaRepositoryMock.Setup(repo => repo.ObterInativoPorIdAsync(1)).ReturnsAsync((Turma?)null);

        var turmaService = CriarService();

        Func<Task> acao = async () => await turmaService.ReativarTurmaAsync(1);

        await acao.Should().ThrowAsync<EntidadeNaoEncontradaException>()
            .WithMessage("Turma não encontrada.");

        _turmaRepositoryMock.Verify(repo => repo.ReativarAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EditarTurma_ComCapacidadeAbaixoDosAlunosAtivos_DeveLancarRegraDeNegocioExceptionENaoEditar()
    {
        _turmaRepositoryMock.Setup(repo => repo.ObterPorIdAsync(1)).ReturnsAsync(CriarTurma());
        _turmaRepositoryMock.Setup(repo => repo.ValidarPelosIdentificadores(
            It.IsAny<string>(), It.IsAny<SerieEnum>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
        _turmaRepositoryMock.Setup(repo => repo.ContarAlunosAtivosAsync(1)).ReturnsAsync(20);

        var turmaService = CriarService();

        Func<Task> acao = async () => await turmaService.EditarTurmaAsync(CriarDtoEdicao(capacidade: 19));

        await acao.Should().ThrowAsync<RegraDeNegocioException>()
            .WithMessage("A capacidade não pode ser menor que o número de alunos ativos na turma. Capacidade informada: 19; alunos ativos: 20.");

        _turmaRepositoryMock.Verify(repo => repo.EditarAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task EditarTurma_ComCapacidadeIgualAosAlunosAtivos_DeveEditar()
    {
        _turmaRepositoryMock.Setup(repo => repo.ObterPorIdAsync(1)).ReturnsAsync(CriarTurma());
        _turmaRepositoryMock.Setup(repo => repo.ValidarPelosIdentificadores(
            It.IsAny<string>(), It.IsAny<SerieEnum>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
        _turmaRepositoryMock.Setup(repo => repo.ContarAlunosAtivosAsync(1)).ReturnsAsync(20);

        var turmaService = CriarService();

        await turmaService.EditarTurmaAsync(CriarDtoEdicao(capacidade: 20));

        _turmaRepositoryMock.Verify(repo => repo.EditarAsync(
            It.Is<Turma>(t => t.Id == 1 && t.Capacidade == 20)), Times.Once);
    }
}
