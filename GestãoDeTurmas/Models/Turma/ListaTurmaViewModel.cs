using Common.Enums;
using Common.Utils;

namespace GestãoDeTurmas.Models.Turma;

public class ListaTurmaViewModel
{
    public int TurmaId { get; set; }
    public string Identificador { get; set; }
    public SerieEnum Serie { get; set; }
    public TurnoEnum Turno { get; set; }
    public int AnoLetivo { get; set; }
    public int QuantidadeAlunos { get; set; }
    public int QuantidadeDisciplinas { get; set; }
    public string NomeExibicao => $"{Serie.ObterSerieFormatada()} {Identificador}";


}
