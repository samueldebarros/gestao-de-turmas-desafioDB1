using DesafioTecnico1_Fundamentos.Controllers;
using DesafioTecnico1_Fundamentos.Services;

namespace DesafioTecnico1_Fundamentos.Utils.Menus;

internal class MenuGerenciarTurma
{
    public void Exibir(TurmaController turmaController, MatriculaService matriculaService, AlunoController alunoController, DocenteController docenteController)
    {
        int opcao = -1;
        while (opcao != 0)
        {
            Console.Clear();
            Console.WriteLine("Gerenciar de Turma");
            Console.WriteLine("\nO que deseja fazer?");
            Console.WriteLine("1 - Listar Turmas");
            Console.WriteLine("2 - Cadastrar Turma");
            Console.WriteLine("3 - Matricular aluno a uma turma");
            Console.WriteLine("4 - Remover aluno de uma turma");
            Console.WriteLine("5 - Adicionar docente a uma turma");
            Console.WriteLine("6 - Remover docente de uma turma");
            Console.WriteLine("7 - Listar alunos de uma turma");
            Console.WriteLine("0 - Voltar");

            Console.WriteLine("\nSelecione uma opção: ");
            opcao = int.Parse(Console.ReadLine());

            switch (opcao)
            {
                case 1:
                    turmaController.ListarTurmas();
                    break;
                case 2:
                    Console.WriteLine("Digite o ID da turma: ");
                    int id = int.Parse(Console.ReadLine());

                    Console.WriteLine("Digite o nome da turma:");
                    string nome = Console.ReadLine();

                    turmaController.CadastrarTurma(id, nome);
                    break;
                case 3:
                    matriculaService.MatricularAlunoNaTurma();
                    break;
                case 4:
                    matriculaService.DesmatricularAlunoDaTurma();
                    break;
                case 5:
                    matriculaService.AlocarDocenteNaTurma();
                    break;
                case 6:
                    matriculaService.DesalocarDocenteDaTurma();
                    break;
                case 7:
                    turmaController.ListarAlunosDeTurma();
                    break;
                case 0:
                    Console.WriteLine("Aperte qualquer tecla para voltar...");
                    Console.ReadKey(true);
                    break;
            }
        }
    }
}
