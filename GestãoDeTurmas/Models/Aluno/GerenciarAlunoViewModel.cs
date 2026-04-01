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
    }
}
