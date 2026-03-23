namespace GestãoDeTurmas.Models.Turma;

public class GradeCurricularViewModel
{
    public int DisciplinaId { get; set; }
    public int DocenteId { get; set; }
    public string Disciplina { get; set; } = string.Empty;
    public string Docente { get; set; } = string.Empty;
}
