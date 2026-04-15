namespace Tests;

using API.DTOs.AlunoDTOs;
using API.Service;
using Common.Domains;
using Common.Enums;
using Common.Exceptions;
using Moq;
using Repository.Repositories;

public class AlunoServiceTests
{
    [Fact]
    public async Task AdicionarAluno_ComDataNascimentoNoFuturo_DeveLancarRegraDeNegocioException()
    {
        var alunoRepositoryMock = new Mock<IAlunoRepository>();
        var alunoService = new AlunoService(alunoRepositoryMock.Object);

        var alunoDto = new AlunoInputDTO
        {
            Nome = "Samuel de BArros",
            Cpf = "663.544.610-90",
            Email = "samuel@teste.com",
            Sexo = SexoEnum.Masculino,
            DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() => 
        alunoService.AdicionarAlunoAsync(alunoDto));

        Assert.Equal("A data de nascimento não pode ser maior ou igual à data atual.", excecao.Message);
        alunoRepositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Aluno>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarAluno_ComDataNascimentoInvalidaMuitoAntiga_DeveLancarRegraDeNegocioException()
    {
        var alunoRepositoryMock = new Mock<IAlunoRepository>();
        var alunoService = new AlunoService(alunoRepositoryMock.Object);

        var alunoDto = new AlunoInputDTO
        {
            Nome = "Samuel de BArros",
            Cpf = "663.544.610-90",
            Email = "samuel@teste.com",
            Sexo = SexoEnum.Masculino,
            DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-200))
        };

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
        alunoService.AdicionarAlunoAsync(alunoDto));

        Assert.Equal("A data de nascimento informada é inválida (idade superior a 120 anos).", excecao.Message);
        alunoRepositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Aluno>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarAluno_ComEmailJaExistente_DeveLancarRegraDeNegocioException()
    {
        var alunoRepositoryMock = new Mock<IAlunoRepository>();
        var emailDuplicado = "samuel@teste.com";
        alunoRepositoryMock.Setup(repo => repo.ExistePeloEmailAsync(emailDuplicado, null))
        .ReturnsAsync(true);

        var alunoService = new AlunoService(alunoRepositoryMock.Object);

        var alunoDto = new AlunoInputDTO
        {
            Nome = "asdsadasdas",
            Cpf = "663.544.610-90",
            Email = emailDuplicado,
            Sexo = SexoEnum.Masculino,
            DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-20))
        };

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() => alunoService.AdicionarAlunoAsync(alunoDto));

        Assert.Equal("Este e-mail já esta em uso.", excecao.Message);
        alunoRepositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Aluno>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarAluno_QuandoGerarMatricularFalhar5Vezes_DeveLancarRegraDeNegocioException()
    {
        var alunoRepositoryMock = new Mock<IAlunoRepository>();
        var alunoService = new AlunoService(alunoRepositoryMock.Object);

        var alunoDto = new AlunoInputDTO
        {
            Nome = "Samuel de barros",
            Cpf = "663.544.610-90",
            Email = "samuel@teste.com",
            Sexo = SexoEnum.Masculino,
            DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-20))
        };

        alunoRepositoryMock.Setup(repo => repo.ExisteMatriculaAsync(It.IsAny<string>()))
        .ReturnsAsync(true);

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() => alunoService.AdicionarAlunoAsync(alunoDto));

        Assert.Equal("Não foi possível gerar uma matrícula única. Tente novamente!!", excecao.Message);
        alunoRepositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Aluno>()), Times.Never);
    }
}
 