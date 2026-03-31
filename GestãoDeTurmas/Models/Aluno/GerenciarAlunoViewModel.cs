using Common.Enums;

namespace GestãoDeTurmas.Models.Aluno
{
    public class GerenciarAlunoViewModel
    {
        public AlunoInputViewModel NovoAluno { get; set; } = new AlunoInputViewModel();
        public List<AlunoListaViewModel> AlunosCadastrados { get; set; } = new List<AlunoListaViewModel>();
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalResultados { get; set; }
        public int TamanhoPagina { get; set; }
        public bool TemPaginaAnterior { get; set; }
        public bool TemProximaPagina { get; set; }
        public DirecaoOrdenacaoEnum? Direcao { get; set; }
        public string? Ordenacao { get; set; }
        public string? PesquisaAtual { get; set; }
        public SexoEnum? SexoAtual { get; set; }
        public bool? AtivoAtual { get; set; }
    }
}
