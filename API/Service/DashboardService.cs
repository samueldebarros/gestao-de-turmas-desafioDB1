using API.DTOs.DashboardDTOs;
using Repository.Repositories;
using Repository.Repositories.DocenteRepository;
using Repository.Repositories.TurmaRepository;

namespace API.Service;

public class DashboardService : IDashboardService
{
    private readonly IAlunoRepository _alunoRepository;
    private readonly IDocenteRepository _docenteRepository;
    private readonly IDisciplinaRepository _disciplinaRepository;
    private readonly ITurmaRepository _turmaRepository;

    public DashboardService(IAlunoRepository alunoRepository, IDocenteRepository docenteRepository,
        IDisciplinaRepository disciplinaRepository, ITurmaRepository turmaRepository)
    {
        _alunoRepository = alunoRepository;
        _docenteRepository = docenteRepository;
        _disciplinaRepository = disciplinaRepository;
        _turmaRepository = turmaRepository;
    }

    public async Task<DashboardDadosDTO> ObterDadosDashboard()
    {
        return new DashboardDadosDTO()
        {
            TotalAlunos = await _alunoRepository.ContarTodosAsync(),
            TotalDisciplinas = await _disciplinaRepository.ContarTodosAsync(),
            TotalDocentes = await _docenteRepository.ContarTodosAsync(),
            TotalTurmas = await _turmaRepository.ContarTodosAsync(),
            AlunosInativos = await _alunoRepository.ContarInativosAsync(),
            DisciplinasInativos = await _disciplinaRepository.ContarInativosAsync(),
            DocentesInativos = await _docenteRepository.ContarInativosAsync()
        };
    }
}

//perguntar
//var taskTotalAlunos = _alunoRepository.ContarTodosAsync();
//var taskTotalDisciplinas = _disciplinaRepository.ContarTodosAsync();
//var taskTotalDocentes = _docenteRepository.ContarTodosAsync();
//var taskTotalTurmas = _turmaRepository.ContarTodosAsync();
//var taskAlunosInativos = _alunoRepository.ContarInativosAsync();
//var taskDisciplinasInativos = _disciplinaRepository.ContarInativosAsync();
//var taskDocentesInativos = _docenteRepository.ContarInativosAsync();

//await Task.WhenAll(taskTotalAlunos, taskTotalDisciplinas, taskTotalDocentes, taskTotalTurmas, taskAlunosInativos,
//    taskDisciplinasInativos, taskDocentesInativos);

//return new DashboardDadosDTO()
//{
//    TotalAlunos = taskTotalAlunos.Result,
//    TotalDisciplinas = taskTotalDisciplinas.Result,
//    TotalDocentes = taskTotalDocentes.Result,
//    TotalTurmas = taskTotalTurmas.Result,
//    AlunosInativos = taskAlunosInativos.Result,
//    DisciplinasInativos = taskDisciplinasInativos.Result,
//    DocentesInativos = taskDocentesInativos.Result
//};