using Common.Enums;
using Repository.Repositories;

namespace API.DTOs.TurmaDTOs;

public class ListaTurmasDTO
{
    public int Id { get; set; }
    public string Identificador { get; set; }
    public int Capacidade { get; set; }
    public SerieEnum Serie { get; set; }
    public TurnoEnum Turno { get; set; }
    public int AnoLetivo { get; set; }
    public int TotalAlunos { get; set; }
    public int TotalDisciplinas { get; set; }
    public bool Ativo { get; set; }

    public List<DocenteSqlDto>? Docentes { get; set; }
    public List<AlunoDaTurmaDTO>? Alunos { get; set; }
}
