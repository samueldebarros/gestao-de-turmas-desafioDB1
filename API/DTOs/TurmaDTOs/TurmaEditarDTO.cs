using Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs.TurmaDTOs;

public class TurmaEditarDTO
{
    public int Id { get; set; }
    public string Identificador { get; set; }
    public TurnoEnum Turno { get; set; }
    public SerieEnum Serie { get; set; }
    public int AnoLetivo { get; set; }
    public int Capacidade { get; set; }
}
