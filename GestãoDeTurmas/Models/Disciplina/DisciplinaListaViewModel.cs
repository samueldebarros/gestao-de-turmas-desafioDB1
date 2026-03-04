namespace GestãoDeTurmas.Models.Disciplina;

public class DisciplinaListaViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public int CargaHoraria { get; set; }
    public string Ementa {  get; set; }
    public bool Ativo { get; set; }
}
