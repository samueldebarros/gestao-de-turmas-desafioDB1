using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domains;

public class Disciplina : IEntidadeInativavel
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public int CargaHoraria { get; set; }
    public string? Ementa { get; set; }
    public ICollection<Docente> Docentes { get; set; } = new List<Docente>();
    public bool Ativo {  get; set; }
}
