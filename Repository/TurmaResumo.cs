using Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public class TurmaResumo
{
    public int Id { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public TurnoEnum Turno { get; set; }
    public SerieEnum Serie { get; set; }
    public int Capacidade { get; set; }
    public int AnoLetivo { get; set; }
    public bool Ativo { get; set; }
    public int QuantidadeAlunos { get; set; }
    public int QuantidadeDisciplinas { get; set; }
}
