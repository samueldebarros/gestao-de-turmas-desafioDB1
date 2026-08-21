using Common.Enums;

namespace API.DTOs.AlunoDTOs;

public record AlunoBuscaRequest(
    int Pagina = 1,
    int TamanhoPagina = 10,
    string? Pesquisa = null,
    SexoEnum? Sexo = null,
    bool? Ativo = null,
    OrdenacaoAlunoEnum? Ordenacao = null,
    DirecaoOrdenacaoEnum? Direcao = null);