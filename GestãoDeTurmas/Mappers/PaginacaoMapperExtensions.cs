using API.DTOs;
using Common.Enums;
using GestãoDeTurmas.Models;

namespace GestãoDeTurmas.Mappers;

public static class PaginacaoMapperExtensions
{
    public static void MapearPaginacao<T>(this ListagemBaseViewModel viewModel, ListaPaginada<T> listaPaginada, string? ordenacao, DirecaoOrdenacaoEnum? direcao)
    {
        viewModel.PaginaAtual = listaPaginada.PaginaAtual;
        viewModel.TotalPaginas = listaPaginada.TotalPaginas;
        viewModel.TotalResultados = listaPaginada.TotalResultados;
        viewModel.TamanhoPagina = listaPaginada.TamanhoPagina;
        viewModel.TemPaginaAnterior = listaPaginada.TemPaginaAnterior;
        viewModel.TemProximaPagina = listaPaginada.TemProximaPagina;

        viewModel.Ordenacao = ordenacao;
        viewModel.Direcao = direcao;
    }
}
