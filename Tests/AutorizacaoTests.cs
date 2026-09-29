namespace Tests;

using FluentAssertions;
using GestãoDeTurmas.Autorizacao;
using GestãoDeTurmas.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

public class AutorizacaoTests
{
    private static IAuthorizationService CriarServico()
    {
        var servicos = new ServiceCollection();
        servicos.AddLogging();
        servicos.AddAuthorization(Politicas.Registrar);

        return servicos.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
    }

    private static ClaimsPrincipal CriarUsuario(string? papel)
    {
        var claims = new List<Claim>();
        if (papel is not null)
            claims.Add(new Claim(ClaimTypes.Role, papel));

        var identidade = new ClaimsIdentity(claims, "teste");
        return new ClaimsPrincipal(identidade);
    }

    [Theory]
    [InlineData(Papeis.Admin, Politicas.Coordenacao, true)]
    [InlineData(Papeis.Coordenador, Politicas.Coordenacao, true)]
    [InlineData(Papeis.Docente, Politicas.Coordenacao, false)]
    [InlineData(null, Politicas.Coordenacao, false)]
    [InlineData(Papeis.Admin, Politicas.ConsultaAlunos, true)]
    [InlineData(Papeis.Coordenador, Politicas.ConsultaAlunos, true)]
    [InlineData(Papeis.Docente, Politicas.ConsultaAlunos, true)]
    [InlineData(null, Politicas.ConsultaAlunos, false)]
    public async Task AvaliarPolicy_ConformeMatrizDePapeis(string? papel, string policy, bool esperado)
    {
        var servico = CriarServico();
        var usuario = CriarUsuario(papel);

        var resultado = await servico.AuthorizeAsync(usuario, policy);

        resultado.Succeeded.Should().Be(esperado);
    }

    private static string[] ObterPoliciesEfetivas(string nomeDaAction)
    {
        var metodo = typeof(AlunosController).GetMethod(nomeDaAction)
            ?? throw new InvalidOperationException($"Action {nomeDaAction} não encontrada em AlunosController");

        var atributosDaClasse = metodo.DeclaringType!.GetCustomAttributes<AuthorizeAttribute>();
        var atributosDoMetodo = metodo.GetCustomAttributes<AuthorizeAttribute>();

        return atributosDaClasse.Concat(atributosDoMetodo)
            .Where(atributo => atributo.Policy is not null)
            .Select(atributo => atributo.Policy!)
            .ToArray();
    }

    [Theory]
    [InlineData(nameof(AlunosController.AdicionarAluno))]
    [InlineData(nameof(AlunosController.InativarAluno))]
    [InlineData(nameof(AlunosController.ReativarAluno))]
    [InlineData(nameof(AlunosController.EditarAluno))]
    [InlineData(nameof(AlunosController.Importar))]
    public void AlunosController_AcoesDeMutacao_ExigemPolicyDeCoordenacao(string nomeDaAction)
    {
        var policies = ObterPoliciesEfetivas(nomeDaAction);

        policies.Should().Contain(Politicas.Coordenacao);
    }

    [Theory]
    [InlineData(nameof(AlunosController.ObterTodosOsAlunos))]
    [InlineData(nameof(AlunosController.BuscarAlunos))]
    public void AlunosController_AcoesDeConsulta_ExigemSomentePolicyDeConsultaAlunos(string nomeDaAction)
    {
        var policies = ObterPoliciesEfetivas(nomeDaAction);

        policies.Should().BeEquivalentTo([Politicas.ConsultaAlunos]);
    }

    [Fact]
    public void AlunosController_NenhumaActionNemAClasseTemAllowAnonymous()
    {
        var tipo = typeof(AlunosController);

        var classeTemAllowAnonymous = tipo.GetCustomAttributes<AllowAnonymousAttribute>().Any();
        var algumMetodoTemAllowAnonymous = tipo
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Any(metodo => metodo.GetCustomAttributes<AllowAnonymousAttribute>().Any());

        classeTemAllowAnonymous.Should().BeFalse();
        algumMetodoTemAllowAnonymous.Should().BeFalse();
    }
}
