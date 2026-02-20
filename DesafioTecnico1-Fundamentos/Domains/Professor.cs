using System;
using System.Collections.Generic;
using System.Text;

namespace DesafioTecnico1_Fundamentos.Domains;

internal class Docente
{

    public int Id { get; set; }
    public string Nome { get; set; }
    public Turma Turma { get; set; }

    public Docente(int id, string nome)
    {
        Id = id;
        Nome = nome;
    }

}
