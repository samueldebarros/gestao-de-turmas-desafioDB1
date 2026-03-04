using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Utils;

public static class ValidacaoCpf
{
    public static bool IsCpfValido(string cpf)
    {
        // Remove caracteres não numéricos
        cpf = Regex.Replace(cpf, @"\D", "");

        // Verifica se tem 11 dígitos
        if (cpf.Length != 11) return false;

        // Rejeita CPFs com todos os dígitos iguais
        if (Regex.IsMatch(cpf, @"^(\d)\1{10}$")) return false;

        // Calcula o primeiro dígito verificador
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += (cpf[i] - '0') * (10 - i);
        }
        int remainder = sum % 11;
        int digit1 = remainder < 2 ? 0 : 11 - remainder;

        // Calcula o segundo dígito verificador
        sum = 0;
        for (int i = 0; i < 10; i++)
        {
            sum += (cpf[i] - '0') * (11 - i);
        }
        remainder = sum % 11;
        int digit2 = remainder < 2 ? 0 : 11 - remainder;

        // Verifica se os dígitos calculados conferem
        return (cpf[9] - '0') == digit1 && (cpf[10] - '0') == digit2;
    }
}

