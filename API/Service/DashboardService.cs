using API.DTOs.DashboardDTOs;
using Common.Utils;
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

    public async Task<List<PainelDemograficoTurmaDTO>> ObterPainelDemograficoPorTurmaAsync()
    {
        var resultados = await _turmaRepository.ObterPainelDemograficoPorTurmaAsync();

        return resultados.Select(r => new PainelDemograficoTurmaDTO
        {
            TurmaId = r.TurmaId,
            Identificador = r.Identificador,
            Serie = (int)r.Serie,
            SerieDescricao = r.Serie.ObterSerieFormatada(),
            Menor15 = r.Menor15,
            De15a17 = r.De15a17,
            Maior18 = r.Maior18,
            IdadeMedia = r.IdadeMedia
        }).ToList();
    }

    public async Task<List<BalancoEvasaoSerieDTO>> ObterBalancoEvasaoPorSerieAsync()
    {
        var resultados = await _turmaRepository.ObterBalancoEvasaoPorSerieAsync();

        return resultados.Select(r => new BalancoEvasaoSerieDTO
        {
            AnoLetivo = r.AnoLetivo,
            Serie = (int)r.Serie,
            SerieDescricao = r.Serie.ObterSerieFormatada(),
            TotalMatriculas = r.TotalMatriculas,
            AlunosAtivos = r.AlunosAtivos,
            AlunosTrancadosOuCancelados = r.AlunosTrancadosOuCancelados,
            PercentualEvasao = r.PercentualEvasao
        }).ToList();
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