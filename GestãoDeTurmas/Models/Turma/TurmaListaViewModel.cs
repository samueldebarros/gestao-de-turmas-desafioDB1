using Common.Enums;
using Common.Utils;

namespace GestãoDeTurmas.Models.Turma;

public class TurmaListaViewModel
{
    public int Id { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public int AnoLetivo { get; set; }
    public SerieEnum Serie { get; set; }
    public TurnoEnum Turno { get; set; }
    public int Capacidade { get; set; }
    public bool Ativo { get; set; }
    public int TotalAlunos { get; set; }
    public int TotalDisciplinas { get; set; }
    public string NomeExibicao => $"{Serie.ObterDescricao()} {Identificador}";

}
