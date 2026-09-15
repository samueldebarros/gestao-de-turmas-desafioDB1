using Common.Enums;

namespace API.DTOs.DocenteDTOs;

public record DocenteBuscaRequest(
    int Pagina = 1,
    int TamanhoPagina = 10,
    string? Pesquisa = null,
    bool? Ativo = null,
    OrdenacaoDocenteEnum? Ordenacao = null,
    DirecaoOrdenacaoEnum? Direcao = null,
    int? DisciplinaId = null);
