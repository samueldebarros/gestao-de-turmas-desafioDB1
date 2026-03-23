using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestãoDeTurmas.Models.Turma;

public class TurmaDetalhesViewModel
{
    public int Id { get; set; }
    public string NomeExibicao { get; set; } = string.Empty;
    public string Turno { get; set; } = string.Empty;
    public int AnoLetivo { get; set; }
    public int Capacidade { get; set; }
    public int TotalMatriculados { get; set; }
    public bool Ativo { get; set; }
    public List<EnturmamentoViewModel> Alunos { get; set; } = new();
    public List<GradeCurricularViewModel> Grade { get; set; } = new();
    public List<SelectListItem> AlunosDisponiveis { get; set; } = new();
    public List<SelectListItem> DisciplinasDisponiveis { get; set; } = new();
    public int AlunoSelecionadoId { get; set; }
    public int DisciplinaSelecionadaId { get; set; }
    public int DocenteSelecionadoId { get; set; }
}
