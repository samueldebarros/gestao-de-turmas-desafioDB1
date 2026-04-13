using Common.Enums;
using GestãoDeTurmas.Views.Shared;

namespace GestãoDeTurmas.Models.Aluno
{
    public class GerenciarAlunoViewModel : ListagemBaseViewModel
    {
        public AlunoInputViewModel NovoAluno { get; set; } = new AlunoInputViewModel();
        public List<AlunoListaViewModel> AlunosCadastrados { get; set; } = new List<AlunoListaViewModel>();
        public string? PesquisaAtual { get; set; }
        public SexoEnum? SexoAtual { get; set; }
        public bool? AtivoAtual { get; set; }
        public void RegistrarFiltros(string? pesquisa, SexoEnum? sexo, bool? ativo)
        {
            PesquisaAtual = pesquisa;
            SexoAtual = sexo;
            AtivoAtual = ativo;

            if (!string.IsNullOrEmpty(pesquisa)) FiltrosAtivos["pesquisa"] = pesquisa;
            if (sexo.HasValue) FiltrosAtivos["sexo"] = sexo.Value.ToString();
            if (ativo.HasValue) FiltrosAtivos["ativo"] = ativo.Value.ToString();
        }
    }
}
