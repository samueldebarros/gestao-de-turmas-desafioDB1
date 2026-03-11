namespace Common.Domains;

public class Docente
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string? Email { get; set; }
    public DateOnly DataNascimento { get; set; }
    public int? DisciplinaId { get; set; }
    public Disciplina? Disciplina { get; set; }
    public bool Ativo { get; set; } = true;
}
