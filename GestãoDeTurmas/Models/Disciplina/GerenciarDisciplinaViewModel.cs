using Common.Enums;
using GestãoDeTurmas.Views.Shared;

namespace GestãoDeTurmas.Models.Disciplina;

public class GerenciarDisciplinaViewModel : ListagemBaseViewModel
{
    public List<DisciplinaListaViewModel> DisciplinasCadastradas { get; set; }
    public DisciplinaInputViewModel NovaDisciplina { get; set; }
    public string? PesquisaAtual { get; set; }
    public bool? AtivoAtual { get; set; }
    public void RegistrarFiltros(string? pesquisa, bool? ativo)
    {
        PesquisaAtual = pesquisa;
        AtivoAtual = ativo;

        if (!string.IsNullOrEmpty(pesquisa)) FiltrosAtivos["pesquisa"] = pesquisa;
        if (ativo.HasValue) FiltrosAtivos["ativo"] = ativo.Value.ToString();
    }
}
