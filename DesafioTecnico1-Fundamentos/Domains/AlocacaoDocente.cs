using System;
using System.Collections.Generic;
using System.Text;

namespace DesafioTecnico1_Fundamentos.Domains;

internal class AlocacaoDocente
{
    public Docente Docente;
    public Turma Turma;

    public AlocacaoDocente(Docente docente, Turma turma)
    {
        Docente = docente;
        Turma = turma;
    }
}
