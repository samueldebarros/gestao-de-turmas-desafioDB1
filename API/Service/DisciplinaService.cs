using API.DTOs.DisciplinaDTOs;
using Common.Domains;
using Common.Exceptions;
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

    public async Task EditarDisciplinaAsync(EditarDisciplinaDTO disciplinaDTO)
    {
        var disciplinaExistente = await _disciplinaRepository.ObterDisciplinaPorIdAsync(disciplinaDTO.Id);

        if (disciplinaExistente == null)
            throw new EntidadeNaoEncontradaException("A disciplina que você tentou editar não foi encontrado.");

        if (!disciplinaExistente.Ativo)
            throw new RegraDeNegocioException("Não é possivel editar uma disciplina inativada.");

        if (disciplinaDTO.CargaHoraria <= 0)
            throw new RegraDeNegocioException("Uma disciplina não pode ter carga horária igual ou inferior a 0");

        disciplinaExistente.Nome = disciplinaDTO.Nome;
        disciplinaExistente.CargaHoraria = disciplinaDTO.CargaHoraria;
        disciplinaExistente.Ementa = disciplinaDTO.Ementa;

        await _disciplinaRepository.EditarDisciplinaAsync(disciplinaExistente);
    }

    public Task InativarDisciplinaAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Disciplina> ObterDisciplinaPorIdAsync(int id)
    {
        return await _disciplinaRepository.ObterDisciplinaPorIdAsync(id);
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
