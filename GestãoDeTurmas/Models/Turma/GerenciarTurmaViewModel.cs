using Common.Enums;

namespace GestãoDeTurmas.Models.Turma;

public class GerenciarTurmaViewModel
{
    public List<ListaTurmaViewModel> Turmas { get; set; }
    public string? Pesquisa { get; set; }
    public OrdenacaoTurmaEnum? Ordenacao { get; set; }
}
