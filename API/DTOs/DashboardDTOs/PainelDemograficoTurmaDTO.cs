namespace API.DTOs.DashboardDTOs;

public class PainelDemograficoTurmaDTO
{
    public int TurmaId { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public int Serie { get; set; }
    public string SerieDescricao { get; set; } = string.Empty;
    public int Menor15 { get; set; }
    public int De15a17 { get; set; }
    public int Maior18 { get; set; }
    public decimal IdadeMedia { get; set; }
}
