namespace Tests;

using API.DTOs;
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
        var parametros = new Dictionary<string, object> { ["capacidade"] = 20, ["alunosAtivos"] = 20 };
        var turmaServiceMock = new Mock<ITurmaService>();
        turmaServiceMock
            .Setup(s => s.AdicionarTurmaAsync(It.IsAny<TurmaInputDTO>()))
            .ThrowsAsync(new RegraDeNegocioException("TURMA_CAPACIDADE_ATINGIDA", "Capacidade da turma excedida.", parametros));

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
        var erro = unprocessable.Value.Should().BeOfType<ErroNegocioDTO>().Subject;
        erro.Mensagem.Should().Be("Capacidade da turma excedida.");
        erro.Codigo.Should().Be("TURMA_CAPACIDADE_ATINGIDA");
        erro.Params.Should().BeEquivalentTo(parametros);
    }
}
