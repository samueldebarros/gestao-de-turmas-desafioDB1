using Common.Enums;

namespace Repository.Relatorios;
public class BalancoEvasaoSerieResultado
{
    public int AnoLetivo { get; set; }
    public SerieEnum Serie { get; set; }
    public int TotalMatriculas { get; set; }
    public int AlunosAtivos { get; set; }
    public int AlunosTrancadosOuCancelados { get; set; }
    public decimal PercentualEvasao { get; set; }
}
