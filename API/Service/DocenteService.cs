using API.DTOs;
using API.DTOs.DocenteDTOs;
using Common.Domains;
using Common.Exceptions;
using Common.Utils;
using Repository;

namespace API.Service;

public class DocenteService : IDocenteService
{
    private readonly IDocenteRepository _docenteRepository;

    public DocenteService(IDocenteRepository docenteRepository)
    {
        _docenteRepository = docenteRepository;
    }
    public async Task<Docente> ObterPeloIdAsync(int id)
    {
        return await _docenteRepository.ObterPeloIdAsync(id);
    }

    private async Task<string> ValidarEProcessarCpfAsync(string cpfSujo)
    {
        var cpfLimpo = ValidacaoCpf.Limpar(cpfSujo);

        if (!ValidacaoCpf.IsCpfValido(cpfLimpo)) throw new RegraDeNegocioException("O CPF informado é invalido");

        if (await _docenteRepository.ExistePeloCpfAsync(cpfLimpo)) 
            throw new RegraDeNegocioException("Esse CPF já esta em uso.");

        return cpfLimpo;
    }

    private void ValidarDataNascimento(DateOnly? dataNascimento)
    {
        if (dataNascimento >= DateOnly.FromDateTime(DateTime.Today))
            throw new RegraDeNegocioException("A data de nascimento não pode ser maior ou igual à data atual.");

        var dataMinimaAceitavel = DateOnly.FromDateTime(DateTime.Today).AddYears(-120);
        if (dataNascimento < dataMinimaAceitavel)
            throw new RegraDeNegocioException("A data de nascimento informada é inválida (idade superior a 120 anos).");
    }

    public async Task AdicionarDocenteAsync(DocenteInputDTO docente)
    {
        ValidarDataNascimento(docente.DataNascimento);

        if (await _docenteRepository.ExistePeloEmailAsync(docente.Email)) 
            throw new RegraDeNegocioException("Este e-mail já esta em uso.");

        var cpfLimpo = await ValidarEProcessarCpfAsync(docente.Cpf);

        Docente novoDocente = new Docente()
        {
            Nome = docente.Nome,
            Cpf = cpfLimpo,
            Email = docente.Email,
            // TODO: Disciplina em AdicionarDocenteDTO Especialidade = docente.Especialidade,
            DataNascimento = (DateOnly)docente.DataNascimento,
            Ativo = true
        };

        await _docenteRepository.AdicionarDocenteAsync(novoDocente);
    }

    public async Task InativarDocenteAsync(int id)
    {
        var docente = await _docenteRepository.ObterPeloIdAsync(id);

        if (docente == null) 
            throw new EntidadeNaoEncontradaException("Erro: O docente a ser inativado não foi encontrado.");

        await _docenteRepository.InativarDocenteAsync(id);
    }
    public async Task ReativarDocenteAsync(int id)
    {
        var docente = await _docenteRepository.ObterInativoPeloIdAsync(id);

        if (docente == null) 
            throw new EntidadeNaoEncontradaException("Erro: O docente a ser reativado não foi encontrado.");

        await _docenteRepository.ReativarDocenteAsync(id);
    }

    public async Task<ListaPaginada<Docente>> ObterTodosOsDocentesAsync(int pagina = 1, int tamanho = 5, string? pesquisa = null, bool? ativo = null)
    {
        var (docentes, total) = await _docenteRepository.ObterTodosOsDocentesAsync(pagina, tamanho, pesquisa,ativo);

        var docentesPaginados = new ListaPaginada<Docente>(docentes, total, pagina, tamanho);

        return docentesPaginados;
    }

    public async Task EditarDocenteAsync(EditarDocenteDTO docente)
    {
        var docenteExistente = await _docenteRepository.ObterPeloIdAsync(docente.Id);

        if (docenteExistente == null)
            throw new EntidadeNaoEncontradaException("O docente que você tentou editar não foi encontrado.");

        if (!docenteExistente.Ativo)
            throw new RegraDeNegocioException("Não é possivel editar um docente inativo.");

        ValidarDataNascimento(docente.DataNascimento);

        if (await _docenteRepository.ExistePeloEmailAsync(docente.Email, docente.Id)) 
            throw new RegraDeNegocioException("Este e-mail já esta em uso.");

        docenteExistente.Nome = docente.Nome;
        docenteExistente.DataNascimento = docente.DataNascimento;
        // TODO: Disciplina em EditarDocente docenteExistente.Especialidade = docente.Especialidade;
        docenteExistente.Email = docente.Email;

        await _docenteRepository.EditarDocenteAsync(docenteExistente);
    }
}
