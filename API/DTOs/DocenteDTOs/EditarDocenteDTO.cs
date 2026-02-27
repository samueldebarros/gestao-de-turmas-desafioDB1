using System.ComponentModel.DataAnnotations;

namespace API.DTOs.DocenteDTOs;

public class EditarDocenteDTO
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string? Email { get; set; }
    public string Especialidade { get; set; }
    public DateOnly DataNascimento { get; set; }
}
