namespace GestãoDeTurmas.Models.Disciplina;

public class GerenciarDisciplinaViewModel
{
    public List<DisciplinaListaViewModel> DisciplinasCadastradas { get; set; }
    public DisciplinaInputViewModel NovaDisciplina { get; set; }

    public int TotalPaginas { get; set; }
    public int PaginaAtual { get; set; }
    public int TamanhoPagina { get; set; }
    public bool TemProximaPagina { get; set; }
    public bool TemPaginaAnterior { get; set; }
    public int TotalResultados { get; set; }

}
