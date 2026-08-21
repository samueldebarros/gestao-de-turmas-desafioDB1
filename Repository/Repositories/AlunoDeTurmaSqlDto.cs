
using Common.Enums;

namespace Repository.Repositories;

public class AlunoDeTurmaSqlDto
{
    public int TurmaId { get; set; }
    public int Id { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? Email { get; set; }
    public SexoEnum Sexo { get; set; }
    public DateOnly DataNascimento { get; set; }
}
