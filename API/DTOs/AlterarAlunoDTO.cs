using Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class AlterarAlunoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string? Email { get; set; }
        public DateOnly? DataNascimento { get; set; }
        public SexoEnum Sexo { get; set; }
    }
}
