using System.ComponentModel.DataAnnotations;

namespace API.DTOs.DocenteDTOs;

public class DocenteInputDTO
{
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string? Email { get; set; }
    public int? DisciplinaId { get; set; }
    [Required(ErrorMessage = "A Data de Nascimento é obrigatória!")]
    public DateOnly? DataNascimento {  get; set; }
}
