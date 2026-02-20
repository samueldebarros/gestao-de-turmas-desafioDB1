using DesafioTecnico1_Fundamentos.Controllers;
using DesafioTecnico1_Fundamentos.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesafioTecnico1_Fundamentos.Services;

internal class MatriculaService
{
    TurmaController TurmaController;
    AlunoController AlunoController;
    DocenteController DocenteController;

    public MatriculaService(TurmaController turmaController, AlunoController alunoController, DocenteController docenteController)
    {
        TurmaController = turmaController;
        AlunoController = alunoController;
        DocenteController = docenteController;
    }

    public Matricula ObterMatriculaAlunoNaTurma()
    {
        Console.WriteLine("Digite o ID do aluno: ");
        int alunoId = int.Parse(Console.ReadLine());

        Console.WriteLine("Digite o ID da Turma:");
        int turmaId = int.Parse(Console.ReadLine());

        Aluno aluno = AlunoController.ObterPorId(alunoId);
        Turma turma = TurmaController.ObterPorId(turmaId);

        return new Matricula(aluno, turma);
    }

    public AlocacaoDocente leituraDadosDocenteTurma()
    {
        Console.WriteLine("Digite o ID do docente: ");
        int docenteId = int.Parse(Console.ReadLine());

        Console.WriteLine("Digite o ID da Turma:");
        int turmaId = int.Parse(Console.ReadLine());

        Docente docente = DocenteController.ObterPorId(docenteId);
        Turma turma = TurmaController.ObterPorId(turmaId);

        return new AlocacaoDocente(docente, turma);
    }

    public void MatricularAlunoNaTurma()
    {
        Matricula matricula = ObterMatriculaAlunoNaTurma();

        matricula.Turma.Alunos.Add(matricula.Aluno);
        matricula.Aluno.Turma = matricula.Turma;
        
        Console.WriteLine("Aluno matriculado com sucesso!");       
    }

    public void DesmatricularAlunoDaTurma()
    {
        Matricula matricula = ObterMatriculaAlunoNaTurma();

        if (matricula.Turma.Alunos.Contains(matricula.Aluno))
        {
            matricula.Turma.Alunos.Remove(matricula.Aluno);
            matricula.Aluno.Turma = null;
            Console.WriteLine("Aluno desmatriculado da turma com sucesso!");
        }
        else
        {
            Console.WriteLine("Aluno não encontrado na turma");
        }
    }

    public void AlocarDocenteNaTurma()
    {
        AlocacaoDocente alocacaoDocente = leituraDadosDocenteTurma();

        if (alocacaoDocente.Turma.Docentes.Contains(alocacaoDocente.Docente))
        {
            Console.WriteLine("O docente já esta alocado nessa turma!");
            Thread.Sleep(3000);
        }
        else
        {
            alocacaoDocente.Turma.Docentes.Add(alocacaoDocente.Docente);
            alocacaoDocente.Docente.Turma = alocacaoDocente.Turma;
            Console.WriteLine("Docente alocado com sucesso!");
        }

    }

    public void DesalocarDocenteDaTurma()
    {
        AlocacaoDocente alocacaoDocente = leituraDadosDocenteTurma();

        if (alocacaoDocente.Turma.Docentes.Contains(alocacaoDocente.Docente))
        {
            alocacaoDocente.Turma.Docentes.Remove(alocacaoDocente.Docente);
            alocacaoDocente.Docente.Turma = null;
            Console.WriteLine("Docente removido da turma com sucesso!");
        }
        else
        {
            Console.WriteLine("Docente não encontrado na turma");
        }
    }



}
