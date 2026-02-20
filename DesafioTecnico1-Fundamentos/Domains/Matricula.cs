using System;
using System.Collections.Generic;
using System.Text;

namespace DesafioTecnico1_Fundamentos.Domains;

internal class Matricula
{
    public Aluno Aluno;
    public Turma Turma;

    public Matricula(Aluno aluno, Turma turma)
    {
        Aluno = aluno;
        Turma = turma;
    }
}
