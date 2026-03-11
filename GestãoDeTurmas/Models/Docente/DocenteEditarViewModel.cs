using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GestãoDeTurmas.Models.Docente;

public class DocenteEditarViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "O nome é obrigatório!")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres!")]
    public string Nome { get; set; }

    public string? Email { get; set; }

    [Required(ErrorMessage = "A Data de Nascimento é obrigatória!")]
    public DateOnly DataNascimento { get; set; }
    public int? DisciplinaId { get; set; }
    public IEnumerable<SelectListItem>? Disciplinas { get; set; }
}
