namespace GestãoDeTurmas.Models.Docente;

public class GerenciarDocenteViewModel
{
    public List<DocenteListaViewModel> DocentesCadastrados { get; set; } = new List<DocenteListaViewModel>();
    public DocenteInputViewModel NovoDocente { get; set; } = new DocenteInputViewModel();
}
