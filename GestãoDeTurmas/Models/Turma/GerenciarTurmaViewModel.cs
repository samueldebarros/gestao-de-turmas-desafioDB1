namespace GestãoDeTurmas.Models.Turma;

public class GerenciarTurmaViewModel
{
    public string Pesquisa { get; set; }
    public bool? Ativo { get; set; }
    public List<TurmaListaViewModel> TurmasCadastradas { get; set; } = new List<TurmaListaViewModel>();
}
