using Common.Enums;

namespace Common.Domains;

public class Turma
{
    public int Id { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public int AnoLetivo { get; set; }
    public SerieEnum Serie { get; set; }
    public int Capacidade { get; set; }
    public TurnoEnum Turno { get; set; }
    public bool Ativo { get; set; }

    public ICollection<Enturmamento> Enturmamentos { get; set; } = new List<Enturmamento>();
    public ICollection<GradeCurricular> GradeCurricular { get; set; } = new List<GradeCurricular>();
}
