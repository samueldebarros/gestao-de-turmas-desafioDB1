namespace API.DTOs;
public class ListaPaginada<T> : List<T>
{
    public int PaginaAtual {  get; private set; }
    public int TotalPaginas { get; private set; }

    public ListaPaginada(List<T> lista, int contagem, int paginaAtual, int tamanhoPagina)
    {
        PaginaAtual = paginaAtual;
        TotalPaginas = (int)Math.Ceiling(contagem / (double)tamanhoPagina);

        this.AddRange(lista);
    }

    public bool TemPaginaAnterior => PaginaAtual > 1;
    public bool TemProximaPagina => PaginaAtual < TotalPaginas;
}
