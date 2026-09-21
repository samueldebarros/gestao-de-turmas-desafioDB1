namespace Tests;

using API.DTOs.TurmaDTOs;
using API.Service;
using Common.Enums;
using Common.Exceptions;
using FluentAssertions;
using GestãoDeTurmas.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class TurmaControllerValidacaoTests
{
    [Fact]
    public async Task AdicionarTurma_ComRegraDeNegocioViolada_DeveRetornarUnprocessableEntityComMensagem()
    {
        var turmaServiceMock = new Mock<ITurmaService>();
        turmaServiceMock
            .Setup(s => s.AdicionarTurmaAsync(It.IsAny<TurmaInputDTO>()))
            .ThrowsAsync(new RegraDeNegocioException("Capacidade da turma excedida."));

        var controller = new TurmasController(turmaServiceMock.Object);

        var novaTurma = new TurmaInputDTO
        {
            Identificador = "A",
            Turno = TurnoEnum.Matutino,
            Serie = SerieEnum.PrimeiroAno,
            AnoLetivo = 2026,
            Capacidade = 30
        };

        var resultado = await controller.AdicionarTurma(novaTurma);

        var unprocessable = resultado.Should().BeOfType<UnprocessableEntityObjectResult>().Subject;
        unprocessable.StatusCode.Should().Be(422);
        unprocessable.Value.Should().Be("Capacidade da turma excedida.");
    }
}
