namespace GestãoDeTurmas.Models.Docente;

public class DocenteListaViewModel
{
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string? Email { get; set; }
    public DateOnly DataNascimento { get; set; }
    public string Especialidade { get; set; }
    public bool Ativo { get; set; } = true;
}
