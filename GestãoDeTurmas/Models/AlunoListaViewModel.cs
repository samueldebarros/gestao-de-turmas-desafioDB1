using Common.Enums;

namespace GestãoDeTurmas.Models
{
    public class AlunoListaViewModel
    {
        public int Id { get; set; }
        public string Matricula { get; set; }
        public string NomeExibicao { get; set; }
        public string Cpf { get; set; }
        public string? Email { get; set; }
        public SexoEnum Sexo {  get; set; }
        public DateOnly DataNascimento { get; set; }
        public bool Ativo { get; set; }
    }
}
