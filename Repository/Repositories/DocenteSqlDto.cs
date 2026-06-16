
namespace Repository.Repositories;

public class DocenteSqlDto
{
    public int Id { get; set; }
    public string DocenteNome { get; set; }
    public string DocenteEmail { get; set; }
    public string DisciplinaNome { get; set; }
    public int CargaHoraria { get; set; }
}
