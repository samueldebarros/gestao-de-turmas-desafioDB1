
using System.ComponentModel.DataAnnotations;

namespace Common.Enums;

public enum SerieEnum
{
    [Display(Name = "1º Ano")]
    PrimeiroAno = 1,

    [Display(Name = "2º Ano")]
    SegundoAno = 2,

    [Display(Name = "3º Ano")]
    TerceiroAno = 3
}
