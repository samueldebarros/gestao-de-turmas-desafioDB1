namespace Tests;

using API.DTOs.DocenteDTOs;
using FluentAssertions;
using System.Reflection;

public class DocenteListaDTOTests
{
    // D-2: o DTO de listagem não carrega Cpf nem DataNascimento (minimização de dado pessoal).
    // Ausência não falha sozinha: este teste reprova tanto se alguém acrescentar um campo
    // quanto se alguém remover um dos cinco combinados com o front.
    private static readonly (string Nome, Type Tipo)[] CamposEsperados =
    [
        ("Id", typeof(int)),
        ("Nome", typeof(string)),
        ("Email", typeof(string)),
        ("DisciplinaNome", typeof(string)),
        ("Ativo", typeof(bool)),
    ];

    [Fact]
    public void DocenteListaDTO_DeveTerExatamenteOsCincoCamposDoContrato()
    {
        var propriedades = typeof(DocenteListaDTO)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (p.Name, p.PropertyType))
            .ToArray();

        propriedades.Should().BeEquivalentTo(CamposEsperados,
            "o contrato de DocenteListaDTO é fechado: nem campo a mais (dado pessoal voltando a trafegar), nem campo a menos (quebra o front)");
    }
}
