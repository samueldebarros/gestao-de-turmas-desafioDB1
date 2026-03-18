
namespace Common.Domains;

public class GradeCurricular
{
    public int TurmaId { get; set; }
    public int DisciplinaId { get; set; }
    public int DocenteId { get; set; }

    public Turma Turma { get; set; } = null!;
    public Disciplina Disciplina { get; set; } = null!;
    public Docente Docente { get; set; } = null!;
}
