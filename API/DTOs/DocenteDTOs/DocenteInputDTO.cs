namespace API.DTOs.DocenteDTOs;

public class DocenteInputDTO
{
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string? Email { get; set; }
    public int? DisciplinaId { get; set; }
    public DateOnly? DataNascimento {  get; set; }
}
