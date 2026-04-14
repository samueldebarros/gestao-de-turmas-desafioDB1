namespace Tests;

public class AlunoTests
{
    [Fact]
    public void NovoAluno_AoSerCriado_DeveEstarAtivo()
    {
        var aluno = new Aluno { Nome = "Samuel" };

        Assert.True(aluno.Ativo, "O aluno deveria nascer com o status Ativo = true");
    }

    [Theory]
    [InlineData("")] 
    [InlineData("123")]     
    [InlineData("123456789012")] 
    public void ValidarCpf_ComCpfsInvalidos_DeveRetornarFalso(string cpfInvalido)
    {
        var aluno = new Aluno();
        var resultado = aluno.ValidarCpf(cpfInvalido);
        Assert.False(resultado, $"O CPF '{cpfInvalido}' deveria ter sido bloqueado.");
    }

    [Fact]
    public void AdicionarAluno_EmTurmaSemEspaco_DeveDarErro()
    {
        var turma = new Turma { CapacidadeMaxima = 0 };
        var aluno = new Aluno { Nome = "João" };
        var erro = Assert.Throws<InvalidOperationException>(() => turma.AdicionarAluno(aluno));

        Assert.Equal("Turma lotada", erro.Message);
    }
}

public class Aluno
{
    public string Nome { get; set; }
    public bool Ativo { get; set; } = true;
    public bool ValidarCpf(string cpf) => cpf != null && cpf.Length == 11;
}
public class Turma
{
    public int CapacidadeMaxima { get; set; }
    public int QuantidadeAtual { get; set; } = 0;
    public void AdicionarAluno(Aluno aluno)
    {
        if (QuantidadeAtual >= CapacidadeMaxima) throw new InvalidOperationException("Turma lotada");
        QuantidadeAtual++;
    }
}
