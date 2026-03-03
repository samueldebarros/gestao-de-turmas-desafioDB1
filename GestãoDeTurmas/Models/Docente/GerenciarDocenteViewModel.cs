namespace GestãoDeTurmas.Models.Docente;

public class GerenciarDocenteViewModel
{
    public List<DocenteListaViewModel> DocentesCadastrados { get; set; } = new List<DocenteListaViewModel>();
    public DocenteInputViewModel NovoDocente { get; set; } = new DocenteInputViewModel();

    public int PaginaAtual { get; set; }
    public int TotalPaginas { get; set; }
    public bool TemProximaPagina { get; set; }
    public bool TemPaginaAnterior { get; set; }
}
