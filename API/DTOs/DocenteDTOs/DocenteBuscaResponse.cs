namespace API.DTOs.DocenteDTOs;

/// <summary>
/// Descreve para o Swagger o envelope montado à mão em DocentesController.BuscarDocentes.
/// Existe apenas como schema: o endpoint devolve um objeto anônimo com estes mesmos campos.
/// </summary>
public record DocenteBuscaResponse(
    List<DocenteListaDTO> Itens,
    int PaginaAtual,
    int TotalPaginas,
    int TotalResultados,
    int TamanhoPagina);
