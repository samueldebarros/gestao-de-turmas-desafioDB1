using Common.Enums;

namespace Common.Domains
{
    public class Aluno
    {
        public int Id { get; set; }
        public string Matricula { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public DateOnly DataNascimento { get; set; }
        public SexoEnum Sexo { get; set; }
        public string? Email { get; set; }
        public bool Ativo { get; set; } = true;
        
      
    }
}
