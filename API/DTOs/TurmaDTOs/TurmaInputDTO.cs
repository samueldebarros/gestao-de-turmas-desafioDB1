using Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs.TurmaDTOs;

public class TurmaInputDTO
{
    public string Identificador { get; set; }
    public TurnoEnum Turno { get; set; }
    public SerieEnum Serie { get; set; }
    public int AnoLetivo { get; set; }
    public int Capacidade { get; set; }
    public List<int> Alocacoes { get; set; } = new();
    public List<int> AlunosIds { get; set; } = new();

}
