namespace GestãoDeTurmas.Models.Docente;

public class GerenciarDocenteViewModel
{
    public List<DocenteListaViewModel> DocentesCadastrados { get; set; } = new List<DocenteListaViewModel>();
    public DocenteInputViewModel NovoDocente { get; set; } = new DocenteInputViewModel();

    public int PaginaAtual { get; set; }
    public int TotalPaginas { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalResultados { get; set; }
    public bool TemProximaPagina { get; set; }
    public bool TemPaginaAnterior { get; set; }
    public string? PesquisaAtual { get; set; }
    public bool? AtivoAtual { get; set; }
}
