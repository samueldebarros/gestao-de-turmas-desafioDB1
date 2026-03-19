using Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utils;

public static class TurnoEnumExtensions
{
    public static string ObterTurnoTranscrito(this TurnoEnum turno)
    {
        return turno switch
        {
            TurnoEnum.Matutino => "Matutino",
            TurnoEnum.Vespertino => "Vespertino",
            TurnoEnum.Noturno => "Noturno",
            _ => "Não informado"
        };
    }
}
