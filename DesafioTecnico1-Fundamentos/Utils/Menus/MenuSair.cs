using System;
using System.Collections.Generic;
using System.Text;

namespace DesafioTecnico1_Fundamentos.Utils.Menus;

internal class MenuSair
{
    public void Exibir()
    {
        Console.WriteLine("Saindo do programa...");
        Console.WriteLine("Pressione qualquer botão");
        Console.ReadKey();
    }
}
