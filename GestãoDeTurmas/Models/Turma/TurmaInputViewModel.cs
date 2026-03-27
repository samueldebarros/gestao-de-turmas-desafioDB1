using Common.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GestãoDeTurmas.Models.Turma;

public class TurmaInputViewModel
{
    [Required(ErrorMessage ="O identificador é obrigatório.")]
    [StringLength(1, ErrorMessage = "O identificador deve ter 1 caracter.")]
    [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "O identificador deve ser uma letra.")]
    public string Identificador { get; set; }
    [Required(ErrorMessage ="Selecione um turno.")]
    public TurnoEnum Turno { get; set; }
    [Required(ErrorMessage ="Selecione uma série.")]
    public SerieEnum Serie { get; set; }
    [Required(ErrorMessage ="O ano letivo é obrigatório.")]
    [Range(2000, 2100, ErrorMessage ="Adicione um ano letivo válido")]
    public int AnoLetivo { get; set; }
    [Required]
    [Range(1,255, ErrorMessage = "Selecione um valor válido (1 a 255).")]
    public int Capacidade { get; set; }
}
