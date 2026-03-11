using Common.Utils;

namespace GestãoDeTurmas.Models.Docente;

public class DocenteListaViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string? Email { get; set; }
    public DateOnly DataNascimento { get; set; }
    public bool Ativo { get; set; } = true;
    public string? NomeDisciplina { get; set; }
    public string CpfFormatado => Cpf.FormatarCpf();
}
