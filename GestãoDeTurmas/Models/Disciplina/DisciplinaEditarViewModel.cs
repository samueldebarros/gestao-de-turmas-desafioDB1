using System.ComponentModel.DataAnnotations;

namespace GestãoDeTurmas.Models.Disciplina;

public class DisciplinaEditarViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage ="O nome é obrigatório")]
    public string Nome { get; set; }
    [Required(ErrorMessage = "A carga horária é obrigatória")]
    public int CargaHoraria { get; set; }
    public string Ementa { get; set; }
}
