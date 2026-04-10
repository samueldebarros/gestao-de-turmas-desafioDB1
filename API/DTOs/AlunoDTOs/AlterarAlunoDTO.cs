using Common.Enums;

namespace API.DTOs.AlunoDTOs
{
    public class EditarAlunoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string? Email { get; set; }
        public DateOnly? DataNascimento { get; set; }
        public SexoEnum Sexo { get; set; }
    }
}
