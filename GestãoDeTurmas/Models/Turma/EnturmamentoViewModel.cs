using Common.Enums;

namespace GestãoDeTurmas.Models.Turma;

public class EnturmamentoViewModel
{
    public int AlunoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public SituacaoEnturmamentoEnum Situacao { get; set; }
}
