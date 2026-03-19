using Common.Enums;

namespace API.DTOs.TurmaDTOs;

public class TurmaInputDTO
{
    public string Identificador { get; set; } = string.Empty;
    public int AnoLetivo { get; set; }
    public SerieEnum Serie { get; set; }
    public int VagasMaximas { get; set; }
}
