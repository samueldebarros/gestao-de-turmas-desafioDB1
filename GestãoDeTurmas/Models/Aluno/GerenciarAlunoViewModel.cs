using Common.Enums;
using GestãoDeTurmas.Views.Shared;

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
        public int InicioPagina => (PaginaAtual - 1) * TamanhoPagina + 1;
        public int FimPagina => Math.Min(PaginaAtual * TamanhoPagina, TotalResultados);


        public DirecaoOrdenacaoEnum? Direcao { get; set; }
        public string? Ordenacao { get; set; }
        public string? PesquisaAtual { get; set; }
        public SexoEnum? SexoAtual { get; set; }
        public bool? AtivoAtual { get; set; }
        public Dictionary<string, string> FiltrosAtivos { get; set; } = new();

        public PaginacaoViewModel MontarPaginacao(string controller, string action)
        {
            var filtros = new Dictionary<string, string>(FiltrosAtivos);

            if (!string.IsNullOrEmpty(Ordenacao))
            {
                filtros["ordenacao"] = Ordenacao;
                filtros["direcao"] = Direcao.ToString();
            }

            return new PaginacaoViewModel
            {
                PaginaAtual = PaginaAtual,
                TotalPaginas = TotalPaginas,
                TemPaginaAnterior = TemPaginaAnterior,
                TemProximaPagina = TemProximaPagina,
                Controller = controller,
                Action = action,
                Filtros = filtros
            };
        }
    }
}
