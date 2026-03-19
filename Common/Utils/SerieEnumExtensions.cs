using Common.Enums;

namespace Common.Utils;

public static class SerieEnumExtensions
{
    public static string ObterDescricao(this SerieEnum serie)
    {
        return serie switch
        {
            SerieEnum.PrimeiroAno => "1º Ano",
            SerieEnum.SegundoAno => "2º Ano",
            SerieEnum.TerceiroAno => "3º Ano",
            _ => "Série não informada"
        };
    }
}
