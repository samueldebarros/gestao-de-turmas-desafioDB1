using Common.Enums;
using GestãoDeTurmas.Views.Shared;

namespace GestãoDeTurmas.Models.Disciplina;

public class GerenciarDisciplinaViewModel : ListagemBaseViewModel
{
    public List<DisciplinaListaViewModel> DisciplinasCadastradas { get; set; }
    public DisciplinaInputViewModel NovaDisciplina { get; set; }
    public string? PesquisaAtual { get; set; }
    public bool? AtivoAtual { get; set; }

}
