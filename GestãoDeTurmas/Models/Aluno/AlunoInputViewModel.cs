using Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestãoDeTurmas.Models.Aluno
{
    public class AlunoInputViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório!")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
        public string Nome { get; set; }
        
        [Required(ErrorMessage = "O CPF é obrigatório!")]
        [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "CPF inválido")]
        public string Cpf { get; set; }

        public string? Email { get; set; }

        [Required(ErrorMessage = "A Data de Nascimento é obrigatória!")]
        public DateOnly? DataNascimento { get; set; }

        [Required(ErrorMessage = "Selecione um sexo.")]
        public SexoEnum Sexo { get; set; }


    }
}
