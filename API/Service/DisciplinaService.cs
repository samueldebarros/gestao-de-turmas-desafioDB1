using API.DTOs;
using API.DTOs.DisciplinaDTOs;
using Common.Domains;
using Common.Exceptions;
using Repository;

namespace API.Service;

public class DisciplinaService : IDisciplinaService
{
    private readonly IDisciplinaRepository _disciplinaRepository;

    public DisciplinaService(IDisciplinaRepository disciplinaRepository)
    {
        _disciplinaRepository = disciplinaRepository;
    }

    public async Task AdicionarDisciplinaAsync(DisciplinaInputDTO disciplina)
    {
        ValidarCargaHoraria(disciplina.CargaHoraria);

        var nomeTratado = disciplina.Nome.Trim();

        if (await _disciplinaRepository.ExistePeloNomeAsync(nomeTratado))
            throw new RegraDeNegocioException("Já existe uma Disciplina com este nome!");

        var novaDisciplina = new Disciplina()
        {
            Nome = nomeTratado,
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
            throw new EntidadeNaoEncontradaException("A disciplina que você tentou editar não foi encontrada.");

        if (!disciplinaExistente.Ativo)
            throw new RegraDeNegocioException("Não é possivel editar uma disciplina inativada.");

        ValidarCargaHoraria(disciplinaDTO.CargaHoraria);

        var nomeTratado = disciplinaDTO.Nome.Trim();

        if (await _disciplinaRepository.ExistePeloNomeAsync(nomeTratado, disciplinaDTO.Id))
            throw new RegraDeNegocioException("Já existe uma Disciplina com este nome!");

        disciplinaExistente.Nome = nomeTratado;
        disciplinaExistente.CargaHoraria = disciplinaDTO.CargaHoraria;
        disciplinaExistente.Ementa = disciplinaDTO.Ementa;

        await _disciplinaRepository.EditarDisciplinaAsync(disciplinaExistente);
    }

    public async Task InativarDisciplinaAsync(int id)
    {
        var disciplina = await ObterDisciplinaPorIdAsync(id);
        if (disciplina == null) 
            throw new EntidadeNaoEncontradaException("A disciplina não foi encontrada.");

        if (await _disciplinaRepository.PossuiDocentesAtivosAsync(id))
            throw new RegraDeNegocioException("Não é possivel inativar uma disciplina com docentes ativos vinculados.");

        await _disciplinaRepository.InativarDisciplinaAsync(id);
    }

    public async Task<Disciplina> ObterDisciplinaPorIdAsync(int id)
    {
        return await _disciplinaRepository.ObterDisciplinaPorIdAsync(id);
    }

    public async Task<List<Disciplina>> ObterDisciplinasAtivasAsync()
    {
        return await _disciplinaRepository.ObterDisciplinasAtivasAsync();
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

    public void ValidarCargaHoraria(int cargaHoraria)
    {
        if (cargaHoraria <= 0)
            throw new RegraDeNegocioException("Uma disciplina não pode ter carga horária igual ou inferior a 0.");
        if (cargaHoraria > 999)
            throw new RegraDeNegocioException("A carga horária informada é inválida.");
    }
}
