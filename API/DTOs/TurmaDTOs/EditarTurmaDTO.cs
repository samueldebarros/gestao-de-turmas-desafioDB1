using Common.Enums;

namespace API.DTOs.TurmaDTOs;

public class EditarTurmaDTO
{
    public int Id { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public int AnoLetivo { get; set; }
    public SerieEnum Serie { get; set; }
    public TurnoEnum Turno { get; set; }
    public int Capacidade { get; set; }
}
