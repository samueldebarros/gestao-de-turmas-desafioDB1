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
}
