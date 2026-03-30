using Common.Enums;

namespace API.DTOs.TurmaDTOs;

public class ListaTurmasDTO
{
    public int TurmaId { get; set; }
    public string Identificador { get; set; }
    public int Capacidade { get; set; }
    public SerieEnum Serie { get; set; }
    public TurnoEnum Turno { get; set; }
    public int AnoLetivo { get; set; }
    public int QuantidadeAlunos { get; set; }
    public int QuantidadeDisciplinas { get; set; }
}
