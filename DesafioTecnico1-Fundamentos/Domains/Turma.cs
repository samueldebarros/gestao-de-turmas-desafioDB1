using System;
using System.Collections.Generic;
using System.Text;

namespace DesafioTecnico1_Fundamentos.Domains;

internal class Turma
{
    public int Id { get; set; }
    public string Nome {  get; set; }
    public List<Aluno> Alunos = new List<Aluno>();
    public List<Docente> Docentes = new List<Docente>();

    public Turma (int id, string nome)
    {
        Id = id;
        Nome = nome;
    }

}
