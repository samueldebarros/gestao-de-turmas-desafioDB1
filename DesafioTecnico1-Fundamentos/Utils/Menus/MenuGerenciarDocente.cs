using DesafioTecnico1_Fundamentos.Controllers;

namespace DesafioTecnico1_Fundamentos.Utils.Menus;

internal class MenuGerenciarDocente
{
    
    public void Exibir(DocenteController docenteController)
    {
        int opcao = -1;
        while (opcao != 0)
        {
            Console.Clear();
            Console.WriteLine("Gerenciar de Docentes");
            Console.WriteLine("\nO que deseja fazer?");
            Console.WriteLine("1 - Listar docentes");
            Console.WriteLine("2 - Cadastrar docente");
            Console.WriteLine("3 - Remover docente");
            Console.WriteLine("4 - Exibir Informaçoes do docente");
            Console.WriteLine("5 - Atualizar informações do docente");
            Console.WriteLine("0 - Voltar");

            Console.WriteLine("\nSelecione uma opção: ");
            opcao = int.Parse(Console.ReadLine());

            switch (opcao)
            {
                case 1:
                    docenteController.ListarDocentes();
                    break;
                case 2:
                    docenteController.CadastrarDocente();
                    break;
                case 3:
                    docenteController.RemoverDocente();
                    break;
                case 4:
                    docenteController.ExibirDocente();
                    break;
                case 5:
                    docenteController.AtualizarDocente();
                    break;
                case 0:
                    Console.WriteLine("Aperte qualquer tecla para voltar...");
                    Console.ReadKey(true);
                    break;
            }
        }
    }
}
