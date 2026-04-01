using Common.Enums;
using GestãoDeTurmas.Views.Shared;

namespace GestãoDeTurmas.Models.Docente;

public class GerenciarDocenteViewModel : ListagemBaseViewModel
{
    public List<DocenteListaViewModel> DocentesCadastrados { get; set; } = new List<DocenteListaViewModel>();
    public DocenteInputViewModel NovoDocente { get; set; } = new DocenteInputViewModel();
    public string? PesquisaAtual { get; set; }
    public bool? AtivoAtual { get; set; }
}
