namespace Tests;

using API.DTOs;
using API.DTOs.DocenteDTOs;
using API.Service;
using Common.Domains;
using Common.Enums;
using FluentAssertions;
using GestãoDeTurmas.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class DocenteBuscaValidacaoTests
{
    // CA-3: DisciplinaId = -1 é a sentinela de UI de "Todas as disciplinas" traduzida no front antes
    // do envio. Se a tradução falhar, o -1 chega ao back e precisa ser rejeitado com 400 — sem essa
    // guarda, o vazamento seria indistinguível de "nenhum filtro" e a tela mostraria a lista inteira.
    [Fact]
    public async Task BuscarDocentes_ComDisciplinaIdNegativo_DeveRetornarBadRequestComMensagemDisciplinaInvalida()
    {
        var docenteServiceMock = new Mock<IDocenteService>();
        var controller = new DocentesController(docenteServiceMock.Object);

        var request = new DocenteBuscaRequest(DisciplinaId: -1);

        var resultado = await controller.BuscarDocentes(request);

        var badRequest = resultado.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().Be("Disciplina Inválida");
    }

    // T5c: o controller lê request.DisciplinaId e repassa como 7º argumento ao service. Sem este
    // teste, trocar esse argumento por null (ou por qualquer outro valor) compila e não quebra
    // nenhum teste existente — o endpoint devolveria a lista inteira sem sinal de erro.
    [Fact]
    public async Task BuscarDocentes_ComDisciplinaIdValido_DeveRepassarDisciplinaIdAoService()
    {
        var docenteServiceMock = new Mock<IDocenteService>();
        docenteServiceMock
            .Setup(s => s.ObterTodosOsDocentesAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(),
                It.IsAny<string?>(), It.IsAny<DirecaoOrdenacaoEnum?>(), It.IsAny<int?>()))
            .ReturnsAsync(new ListaPaginada<Docente>(new List<Docente>(), 0, 1, 10));

        var controller = new DocentesController(docenteServiceMock.Object);
        var request = new DocenteBuscaRequest(DisciplinaId: 3);

        await controller.BuscarDocentes(request);

        docenteServiceMock.Verify(s => s.ObterTodosOsDocentesAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool?>(),
            It.IsAny<string?>(), It.IsAny<DirecaoOrdenacaoEnum?>(), It.Is<int?>(d => d == 3)),
            Times.Once);
    }
}
