using DesafioTecnico1_Fundamentos.Domains;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace DesafioTecnico1_Fundamentos.Controllers;

internal class AlunoController
{
    public List<Aluno> todosOsAlunos = new List<Aluno>();

    public Aluno ObterPorId(int id)
    {
        return todosOsAlunos.FirstOrDefault(aluno => aluno.Id == id);
    }
    

    public void CadastrarAluno(int id, string nome)
    {
        Aluno novoAluno = new Aluno(id, nome);

        todosOsAlunos.Add(novoAluno);
    }

    public void RemoverAluno(Aluno aluno)
    {
        todosOsAlunos.Remove(aluno);
    }

    public void AtualizarAluno(Aluno alunoOriginal, string nomeAlunoAtualizado)
    {
        alunoOriginal.Nome = nomeAlunoAtualizado;
    }
}
