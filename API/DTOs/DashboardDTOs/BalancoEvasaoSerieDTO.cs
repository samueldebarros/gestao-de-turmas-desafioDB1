namespace API.DTOs.DashboardDTOs;

public class BalancoEvasaoSerieDTO
{
    public int AnoLetivo { get; set; }
    public int Serie { get; set; }
    public string SerieDescricao { get; set; } = string.Empty;
    public int TotalMatriculas { get; set; }
    public int AlunosAtivos { get; set; }
    public int AlunosTrancadosOuCancelados { get; set; }
    public decimal PercentualEvasao { get; set; }
}
