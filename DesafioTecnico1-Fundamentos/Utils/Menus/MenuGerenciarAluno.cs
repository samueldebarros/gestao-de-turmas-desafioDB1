using DesafioTecnico1_Fundamentos.Controllers;
using DesafioTecnico1_Fundamentos.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace DesafioTecnico1_Fundamentos.Utils.Menus;

internal class MenuGerenciarAluno
{
    public void Exibir(AlunoController alunoController)
    {
        int opcao = -1;
        while (opcao != 0)
        {
            Console.Clear();
            Console.WriteLine("Gerenciar de Alunos");
            Console.WriteLine("\nO que deseja fazer?");
            Console.WriteLine("1 - Listar alunos");
            Console.WriteLine("2 - Cadastrar Aluno");
            Console.WriteLine("3 - Remover Aluno");
            Console.WriteLine("4 - Exibir Informaçoes do Aluno");
            Console.WriteLine("5 - Atualizar informações do Aluno");
            Console.WriteLine("0 - Voltar");

            Console.WriteLine("\nSelecione uma opção: ");
            opcao = int.Parse(Console.ReadLine());

            switch (opcao)
            {
                case 1:
                    ListarAlunos(alunoController.todosOsAlunos);
                    break;
                case 2:
                    var dados = CapturarDadosCadastro();
                    alunoController.CadastrarAluno(dados.id, dados.nome);
                    break;
                case 3:
                    int id = LerIdAluno();
                    Aluno alunoRemovido = alunoController.ObterPorId(id);
                    alunoController.RemoverAluno(alunoRemovido);
                    break;
                case 4:
                    int idExibido = LerIdAluno();
                    Aluno alunoExibido = alunoController.ObterPorId(idExibido);
                    ExibirAluno(alunoExibido);
                    break;
                case 5:
                    int idAlunoAtualizado = LerIdAluno();
                    Aluno alunoAtualizado = alunoController.ObterPorId(idAlunoAtualizado);

                    Console.WriteLine("Digite as novas informações do aluno:");
                    Console.WriteLine("Nome: ");
                    string novoNome = Console.ReadLine();

                    alunoController.AtualizarAluno(alunoAtualizado, novoNome);
                    break;
                case 0:
                    Console.WriteLine("Aperte qualquer tecla para voltar...");
                    Console.ReadKey(true);
                    break;
            }
        }
    }

    private (int id, string nome) CapturarDadosCadastro()
    {
        Console.WriteLine("Cadastro de aluno");
        Console.WriteLine("---------------------------");
        Console.WriteLine("Digite o ID do aluno: ");
        int id = int.Parse(Console.ReadLine());

        Console.WriteLine("Digite o nome do aluno:");
        string nome = Console.ReadLine();

        return (id, nome);
    }

    private int LerIdAluno()
    {
        Console.WriteLine("Digite o ID do aluno: ");
        int alunoId = int.Parse(Console.ReadLine());

        return alunoId;
    }


    public void ExibirAluno(Aluno aluno)
    {
        Console.WriteLine("Informações do aluno:");
        Console.WriteLine("-----------------------");
        Console.WriteLine($"ID: {aluno.Id} Nome: {aluno.Nome}\n");
        Thread.Sleep(1000);
    }

    public void ListarAlunos(List<Aluno> listaAlunos)
    {
        Console.WriteLine("\nListando todos os Alunos");

        foreach (Aluno aluno in listaAlunos)
        {
            Console.WriteLine("---------------------------");
            Console.WriteLine($"ID do aluno: {aluno.Id}\nNome do aluno: {aluno.Nome}");
        }

        Console.WriteLine("---------------------------");
        Console.WriteLine("Aperte qualquer botão para voltar");
        Console.ReadKey(true);
    }

}
