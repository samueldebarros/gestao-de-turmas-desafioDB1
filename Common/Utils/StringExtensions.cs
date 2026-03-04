using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utils;

public static class StringExtensions
{
    public static string FormatarCpf(this string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf) || cpf.Length != 11) return cpf;

        return Convert.ToUInt64(cpf).ToString(@"000\.000\.000\-00");
    }
}
