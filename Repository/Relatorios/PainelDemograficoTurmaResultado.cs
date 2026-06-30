using Common.Enums;

namespace Repository.Relatorios;

public class PainelDemograficoTurmaResultado
{
    public int TurmaId { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public SerieEnum Serie { get; set; }
    public int Menor15 { get; set; }
    public int De15a17 { get; set; }
    public int Maior18 { get; set; }
    public decimal IdadeMedia { get; set; }
}
