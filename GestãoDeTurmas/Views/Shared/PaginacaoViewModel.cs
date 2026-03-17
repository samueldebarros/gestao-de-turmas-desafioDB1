namespace GestãoDeTurmas.Views.Shared;

public class PaginacaoViewModel
{
    public int PaginaAtual { get; set; }
    public int TotalPaginas { get; set; }
    public bool TemPaginaAnterior { get; set; }
    public bool TemProximaPagina { get; set; }
    public string Action { get; set; } = "Index";
    public string Controller { get; set; } = "";
    public Dictionary<string, string> Filtros { get; set; } = new();

    public static Dictionary<string, string> MontarFiltros(params (string chave, object? valor)[] filtros)
    {
        return filtros
            .Where(f => f.valor != null && f.valor.ToString() != "")
            .ToDictionary(f => f.chave, f => f.valor!.ToString()!);
    }

    public Dictionary<string, string> FiltrosComPagina(int pagina)
    {
        var resultado = new Dictionary<string, string>(Filtros);
        resultado["pagina"] = pagina.ToString();
        return resultado;
    }

    public IEnumerable<int?> PaginasVisiveis(int janela = 1)
    {
        var paginas = new List<int?>();
        var inicio = Math.Max(2, PaginaAtual - janela);
        var fim = Math.Min(TotalPaginas - 1, PaginaAtual + janela);

        paginas.Add(1);

        if (inicio > 2)
            paginas.Add(null);

        for (int i = inicio; i <= fim; i++)
            paginas.Add(i);

        if (fim < TotalPaginas - 1)
            paginas.Add(null);

        if (TotalPaginas > 1)
            paginas.Add(TotalPaginas);

        return paginas;
    }
}
