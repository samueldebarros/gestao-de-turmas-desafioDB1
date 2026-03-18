
using Common.Enums;

namespace Common.Domains;

public class Enturmamento
{
    public int AlunoId { get; set; }
    public int TurmaId { get; set; }
    public DateTime DataEnturmamento { get; set; }
    public SituacaoEnturmamentoEnum Situacao { get; set; }

    public Aluno Aluno { get; set; } = null!;
    public Turma Turma { get; set; } = null!;
 }
