namespace Tests;

using Common.Domains;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Repository.Repositories.DocenteRepository;

public class DocenteBuscaPorDisciplinaTests
{
    private static GestaoEscolarContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<GestaoEscolarContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GestaoEscolarContext(opcoes);
    }

    private static Docente NovoDocente(int id, string nome, int? disciplinaId) => new Docente
    {
        Id = id,
        Nome = nome,
        Cpf = $"000000000{id:00}",
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
        DisciplinaId = disciplinaId,
        Ativo = true
    };

    // CA-1: DisciplinaId = n deve devolver exatamente os docentes daquela disciplina, e nenhum de outra.
    [Fact]
    public async Task ObterTodosOsDocentes_ComDisciplinaIdInformado_DeveRetornarApenasDocentesDaquelaDisciplina()
    {
        using var context = CriarContexto();
        context.Docentes.AddRange(
            NovoDocente(1, "Ana", disciplinaId: 10),
            NovoDocente(2, "Bruno", disciplinaId: 10),
            NovoDocente(3, "Carlos", disciplinaId: 20));
        await context.SaveChangesAsync();

        var repository = new DocenteRepository(context);

        var (docentes, total) = await repository.ObterTodosOsDocentesAsync(disciplinaId: 10);

        total.Should().Be(2);
        docentes.Select(d => d.Id).Should().BeEquivalentTo([1, 2]);
    }

    // CA-2: DisciplinaId = 0 é o valor sentinela para "sem disciplina". A implementação ingênua
    // (comparar DisciplinaId == 0 em vez de == null) devolveria zero docentes; por isso a asserção
    // precisa identificar quem voltou, não só contar.
    [Fact]
    public async Task ObterTodosOsDocentes_ComDisciplinaIdZero_DeveRetornarApenasDocentesSemDisciplina()
    {
        using var context = CriarContexto();
        context.Docentes.AddRange(
            NovoDocente(1, "Ana", disciplinaId: 10),
            NovoDocente(2, "Bruno", disciplinaId: null),
            NovoDocente(3, "Carlos", disciplinaId: null));
        await context.SaveChangesAsync();

        var repository = new DocenteRepository(context);

        var (docentes, total) = await repository.ObterTodosOsDocentesAsync(disciplinaId: 0);

        total.Should().Be(2);
        docentes.Select(d => d.Id).Should().BeEquivalentTo([2, 3]);
    }

    // CA-5: DisciplinaId = null preserva contagem e ordem de antes da fatia (nenhum filtro aplicado).
    [Fact]
    public async Task ObterTodosOsDocentes_SemDisciplinaId_DeveManterMesmaContagemEOrdemDeAntesDaFatia()
    {
        using var context = CriarContexto();
        context.Docentes.AddRange(
            NovoDocente(1, "Carlos", disciplinaId: 20),
            NovoDocente(2, "Ana", disciplinaId: 10),
            NovoDocente(3, "Bruno", disciplinaId: null));
        await context.SaveChangesAsync();

        var repository = new DocenteRepository(context);

        var (docentes, total) = await repository.ObterTodosOsDocentesAsync(disciplinaId: null);

        total.Should().Be(3);
        docentes.Select(d => d.Id).Should().Equal(2, 3, 1);
    }
}
