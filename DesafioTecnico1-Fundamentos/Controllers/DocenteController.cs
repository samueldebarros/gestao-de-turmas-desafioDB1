using DesafioTecnico1_Fundamentos.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace DesafioTecnico1_Fundamentos.Controllers;

internal class DocenteController
{
    public List<Docente> todosOsDocentes = new List<Docente>();

    public Docente ObterPorId(int id)
    {
        return todosOsDocentes.FirstOrDefault(docente => docente.Id == id);
    }

    public Docente leituraDados()
    {
        Console.WriteLine("Digite o ID do docente: ");
        int docenteId = int.Parse(Console.ReadLine());

        Docente docenteLido = ObterPorId(docenteId);
        return docenteLido;
    }

    public void ListarDocentes()
    {
        Console.WriteLine("\nListando todos os docentes");

        foreach (Docente docente in todosOsDocentes)
        {
            Console.WriteLine("---------------------------");
            Console.WriteLine($"ID do Docente: {docente.Id}\nNome: {docente.Nome}");
        }

        Console.WriteLine("---------------------------");
        Console.WriteLine("Aperte qualquer botão para voltar");
        Console.ReadKey(true);
    }

    public void CadastrarDocente()
    {
        Console.WriteLine("Cadastro de docente");
        Console.WriteLine("---------------------------");
        Console.WriteLine("Digite o ID do docente: ");
        int id = int.Parse(Console.ReadLine());

        Console.WriteLine("Digite o nome do docente:");
        string nome = Console.ReadLine();

        Docente novoDocente = new Docente(id, nome);

        todosOsDocentes.Add(novoDocente);

        Console.WriteLine("Docente cadastrado com sucesso!");
        Console.ReadKey(true);
    }

    public void RemoverDocente()
    {
        Docente docente = leituraDados();
        todosOsDocentes.Remove(docente);
    }

    public void ExibirDocente()
    {
        Docente docente = leituraDados();

        Console.WriteLine("Informações do docente:");
        Console.WriteLine("-----------------------");
        Console.WriteLine($"ID: {docente.Id} Nome: {docente.Nome}\n");
        Thread.Sleep(1000);
    }

    public void AtualizarDocente()
    {
        Docente docente = leituraDados();

        Console.WriteLine("Digite as novas informações do Docente:");
        Console.WriteLine("Nome: ");
        string novoNome = Console.ReadLine();

        docente.Nome = novoNome;
        Console.WriteLine("Informações do docente alteradas com sucesso!");
    }
}
