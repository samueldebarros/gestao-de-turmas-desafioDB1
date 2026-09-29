using Microsoft.AspNetCore.Authorization;

namespace GestãoDeTurmas.Autorizacao;

public static class Papeis
{
    public const string Admin = "Admin";
    public const string Coordenador = "Coordenador";
    public const string Docente = "Docente";
}

public static class Politicas
{
    public const string Coordenacao = "Coordenacao";
    public const string ConsultaAlunos = "ConsultaAlunos";

    public static void Registrar(AuthorizationOptions opcoes)
    {
        string[] ComAdmin(params string[] papeis) => [.. papeis, Papeis.Admin];

        opcoes.AddPolicy(Coordenacao, politica =>
            politica.RequireRole(ComAdmin(Papeis.Coordenador)));

        opcoes.AddPolicy(ConsultaAlunos, politica =>
            politica.RequireRole(ComAdmin(Papeis.Coordenador, Papeis.Docente)));
    }
}
