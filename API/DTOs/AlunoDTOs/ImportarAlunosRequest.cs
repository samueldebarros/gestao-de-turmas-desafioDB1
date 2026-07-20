namespace API.DTOs.AlunoDTOs;

public class ImportarAlunosRequest
{
    public IReadOnlyList<AlunoInputDTO> Alunos { get; set; } = [];
}
