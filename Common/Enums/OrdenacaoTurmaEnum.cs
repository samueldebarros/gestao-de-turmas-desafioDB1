using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Common.Enums;

public enum OrdenacaoTurmaEnum
{
    [Display(Name ="Ano Letivo")]
    AnoLetivo = 1,

    [Display(Name = "Série")]
    Serie = 2,

    [Display(Name = "Turno")]
    Turno = 3
}
