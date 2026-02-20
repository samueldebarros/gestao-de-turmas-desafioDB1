using API.DTOs;
using Common.Domains;

namespace GestãoDeTurmas.Models
{
    public class GerenciarAlunoViewModel
    {
        public AlunoInputViewModel NovoAluno { get; set; } = new AlunoInputViewModel();
        public List<AlunoListaViewModel> AlunosCadastrados { get; set; } = new List<AlunoListaViewModel>();
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public bool TemPaginaAnterior { get; set; }
        public bool TemProximaPagina { get; set; }
    }
}
