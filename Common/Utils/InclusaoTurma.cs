using Common.Enums;

namespace Common.Utils;

public class InclusaoTurma
{
    public static bool TentarInterpretar(string? incluir, out InclusaoTurmaEnum flags)
    {
        flags = InclusaoTurmaEnum.Nenhum;

        if (string.IsNullOrWhiteSpace(incluir))
            return true;

        var termos = incluir.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var termo in termos)
        {
            switch (termo.ToLowerInvariant())
            {
                case "docentes": flags |= InclusaoTurmaEnum.Docentes; break;
                case "alunos": flags |= InclusaoTurmaEnum.Alunos; break;
                default: return false;
            }
        }

        return true;
    }
}
