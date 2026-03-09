using API.DTOs;
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

    public async Task InativarDisciplinaAsync(int id)
    {
        var disciplina = await ObterDisciplinaPorIdAsync(id);
        if (disciplina == null) 
            throw new EntidadeNaoEncontradaException("A disciplina não foi encontrada.");

        await _disciplinaRepository.InativarDisciplinaAsync(id);
    }

    public async Task<Disciplina> ObterDisciplinaPorIdAsync(int id)
    {
        return await _disciplinaRepository.ObterDisciplinaPorIdAsync(id);
    }

    public async Task<Disciplina> ObterInativoPorIdAsync(int id)
    {
        return await _disciplinaRepository.ObterInativoPorIdAsync(id);
    }

    public async Task<ListaPaginada<Disciplina>> ObterTodasAsDisciplinasAsync(int pagina = 1, int tamanho = 5, string? pesquisa = null, bool? ativo = null)
    {
        var (disciplinas, total) = await _disciplinaRepository.ObterTodasAsDisciplinasAsync(pagina, tamanho, pesquisa,ativo);

        var disciplinasPaginadas = new ListaPaginada<Disciplina>(disciplinas, total, pagina, tamanho);

        return disciplinasPaginadas;
    }

    public async Task ReativarDisciplinaAsync(int id)
    {
        var disciplina = await ObterInativoPorIdAsync(id);
        if (disciplina == null) 
            throw new EntidadeNaoEncontradaException("A disciplina não foi encontrada.");

        await _disciplinaRepository.ReativarDisciplinaAsync(id);
    }
}
