using API.DTOs.DisciplinaDTOs;
using Common.Domains;
using Repository;

namespace API.Service;

public class DisciplinaService : IDisciplinaService
{
    public readonly IDisciplinaRepository _disciplinaRepository;

    public DisciplinaService(IDisciplinaRepository disciplinaRepository)
    {
        _disciplinaRepository = disciplinaRepository;
    }

    public async Task AdicionarDisciplinaAsync(DisciplinaInputDTO disciplina)
    {
        var novaDisciplina = new Disciplina()
        {
            Nome = disciplina.Nome,
            CargaHoraria = disciplina.CargaHoraria,
            Ementa = disciplina.Ementa,
            Ativo = true
        };

        await _disciplinaRepository.AdicionarDisciplinaAsync(novaDisciplina);
    }

    public Task EditarDisciplinaAsync()
    {
        throw new NotImplementedException();
    }

    public Task InativarDisciplinaAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Disciplina> ObterDisciplinaPorIdAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Disciplina> ObterInativoPorIdAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<Disciplina>> ObterTodasAsDisciplinasAsync()
    {
        return await _disciplinaRepository.ObterTodasAsDisciplinasAsync();
    }

    public Task ReativarDisciplinaAsync()
    {
        throw new NotImplementedException();
    }
}
