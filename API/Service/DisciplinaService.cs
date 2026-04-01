using API.DTOs;
using API.DTOs.DisciplinaDTOs;
using Common.Domains;
using Common.Enums;
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
        var nomeTratado = disciplina.Nome.Trim();

        await ValidarDadosDisciplinaAsync(nomeTratado, disciplina.CargaHoraria);

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
        var disciplinaExistente = await ObterDisciplinaAtivaOuLancarErroAsync(disciplinaDTO.Id);

        if (!disciplinaExistente.Ativo)
            throw new RegraDeNegocioException("Não é possivel editar uma disciplina inativada.");

        var nomeTratado = disciplinaDTO.Nome.Trim();

        await ValidarDadosDisciplinaAsync(nomeTratado, disciplinaDTO.CargaHoraria, disciplinaDTO.Id);

        disciplinaExistente.Nome = nomeTratado;
        disciplinaExistente.CargaHoraria = disciplinaDTO.CargaHoraria;
        disciplinaExistente.Ementa = disciplinaDTO.Ementa;

        await _disciplinaRepository.EditarDisciplinaAsync(disciplinaExistente);
    }

    public async Task InativarDisciplinaAsync(int id)
    {
        await ObterDisciplinaAtivaOuLancarErroAsync(id);

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

    public async Task<ListaPaginada<Disciplina>> ObterTodasAsDisciplinasAsync(int pagina = 1, int tamanho = 5, string? pesquisa = null, bool? ativo = null, string? ordenacao = null,
            DirecaoOrdenacaoEnum? direcao = null)
    {
        var (disciplinas, total) = await _disciplinaRepository.ObterTodasAsDisciplinasAsync(pagina, tamanho, pesquisa,ativo, ordenacao, direcao);

        var disciplinasPaginadas = new ListaPaginada<Disciplina>(disciplinas, total, pagina, tamanho);

        return disciplinasPaginadas;
    }

    public async Task ReativarDisciplinaAsync(int id)
    {
        await ObterDisciplinaInativaOuLancarErroAsync(id);
        await _disciplinaRepository.ReativarDisciplinaAsync(id);
    }

    private void ValidarCargaHoraria(int cargaHoraria)
    {
        if (cargaHoraria <= 0)
            throw new RegraDeNegocioException("Uma disciplina não pode ter carga horária igual ou inferior a 0.");
        if (cargaHoraria > 999)
            throw new RegraDeNegocioException("A carga horária informada é inválida.");
    }

    private async Task<Disciplina> ObterDisciplinaAtivaOuLancarErroAsync(int id)
    {
        var disciplina = await _disciplinaRepository.ObterDisciplinaPorIdAsync(id);
        if (disciplina == null)
            throw new EntidadeNaoEncontradaException("A disciplina não foi encontrada.");
        return disciplina;
    }

    private async Task<Disciplina> ObterDisciplinaInativaOuLancarErroAsync(int id)
    {
        var disciplina = await _disciplinaRepository.ObterInativoPorIdAsync(id);
        if (disciplina == null)
            throw new EntidadeNaoEncontradaException("A disciplina não foi encontrada.");
        return disciplina;
    }

    private async Task ValidarDadosDisciplinaAsync(string nomeTratado, int cargaHoraria, int? ignorarId = null)
    {
        ValidarCargaHoraria(cargaHoraria);

        if (await _disciplinaRepository.ExistePeloNomeAsync(nomeTratado, ignorarId))
            throw new RegraDeNegocioException("Já existe uma Disciplina com este nome!");
    }
}
