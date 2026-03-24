using API.DTOs.TurmaDTOs;
using Common.Domains;
using Common.Enums;

namespace API.Service;

public interface ITurmaService
{

    Task VincularDisciplinaAsync(VincularDisciplinaDTO dto);
    Task MatricularAlunoAsync(MatricularAlunoDTO dto);
    Task<List<Turma>> ObterTodasAsTurmasAsync(string? pesquisa = null, bool? ativo = null, OrdenacaoTurmaEnum? ordenacao = null);
    Task<Turma> ObterPorIdAsync(int id);
    Task AdicionarTurmaAsync(TurmaInputDTO turmaDto);
    Task EditarTurmaAsync(EditarTurmaDTO turmaDto);
    Task InativarTurmaAsync(int id);
    Task ReativarTurmaAsync(int id);
    Task<Turma?> ObterTurmaComDetalhesAsync(int id);
    Task<List<Aluno>> ObterAlunosDisponiveisParaTurmaAsync(int turmaId);
    Task<List<Disciplina>> ObterDisciplinasDisponiveisParaTurmaAsync(int turmaId);
    Task<List<Docente>> ObterDocentesPorDisciplinaAsync(int disciplinaId);

}
