using DesafioTecnico1_Fundamentos.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace DesafioTecnico1_Fundamentos.Controllers;

internal class TurmaController
{
    public List<Turma> todasAsTurmas = new List<Turma>();

    public Turma ObterPorId(int id)
    {
        return todasAsTurmas.FirstOrDefault(turma => turma.Id == id);
    }

    public int LerIdTurma()
    {
        Console.WriteLine("Digite o ID da Turma:");
        int turmaId = int.Parse(Console.ReadLine());
        return turmaId;
    }

    public void ListarTurmas()
    {
        Console.WriteLine("\nListando todas as Turmas");

        foreach (Turma turma in todasAsTurmas)
        {
            Console.WriteLine("---------------------------");
            Console.WriteLine($"ID da Turma: {turma.Id}\nNome da Turma: {turma.Nome}");
        }

        Console.WriteLine("---------------------------");
        Console.WriteLine("Aperte qualquer botão para voltar");
        Console.ReadKey(true);
    }

    public void CadastrarTurma(int id, string nome)
    {
        Turma novaTurma = new Turma(id, nome);

        todasAsTurmas.Add(novaTurma);

        Console.WriteLine("Turma cadastrada com sucesso!");
        Console.ReadKey(true);

    }

    public void RemoverAlunoDaTurma(Turma turma, Aluno aluno)
    {
        turma.Alunos.Remove(aluno);
        aluno.Turma = null;
    }

    public void ListarAlunosDeTurma()
    {
        int idTurma = LerIdTurma();
        Turma turma = ObterPorId(idTurma);

        Console.WriteLine($"Listando alunos da Turma:{turma.Nome}");
        foreach (Aluno aluno in turma.Alunos)
        {
            Console.WriteLine("---------------------------");
            Console.WriteLine($"ID do aluno: {aluno.Id}\nNome do aluno: {aluno.Nome}");
        }
        Console.WriteLine("Digite qualquer tecla para continuar...");
        Console.ReadKey(true);
    }
}
