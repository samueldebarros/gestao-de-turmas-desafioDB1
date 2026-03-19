using Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestãoDeTurmas.Models.Turma;

public class TurmaInputViewModel
{
    [Required(ErrorMessage = "O identificador é obrigatório.")]
    [MaxLength(1, ErrorMessage = "O identificador deve ter apenas 1 caractere.")]
    public string Identificador { get; set; } = string.Empty;

    [Required(ErrorMessage = "O ano letivo é obrigatório.")]
    [Range(2000, 2100, ErrorMessage = "Informe um ano letivo válido.")]
    public int AnoLetivo { get; set; }

    [Required(ErrorMessage = "A série é obrigatória.")]
    public SerieEnum Serie { get; set; }
    [Required(ErrorMessage = "O turno é obrigatório.")]
    public TurnoEnum Turno { get; set; }

    [Required(ErrorMessage = "O número de vagas é obrigatório.")]
    [Range(1, 255, ErrorMessage = "O número de vagas deve ser entre 1 e 255.")]
    public int Capacidade { get; set; }
}

