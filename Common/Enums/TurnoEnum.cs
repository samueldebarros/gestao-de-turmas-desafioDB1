using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Common.Enums;

public enum TurnoEnum
{
    [Display(Name = "Matutino")]
    Matutino = 1,

    [Display(Name = "Vespertino")]
    Vespertino = 2,

    [Display(Name = "Noturno")]
    Noturno = 3
}
