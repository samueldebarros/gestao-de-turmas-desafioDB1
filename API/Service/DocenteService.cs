using API.DTOs;
using API.DTOs.DocenteDTOs;
using Common.Domains;
using Common.Exceptions;
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

    public async Task AdicionarDocenteAsync(DocenteInputDTO docente)
    {
        if (docente.DataNascimento >= DateOnly.FromDateTime(DateTime.Today))
            throw new RegraDeNegocioException("A data de nascimento não pode ser maior ou igual a data de atual");

        if (await _docenteRepository.ExistePeloCpfAsync(docente.Cpf)) 
            throw new RegraDeNegocioException("Este CPF já esta em uso.");

        Docente novoDocente = new Docente()
        {
            Nome = docente.Nome,
            Cpf = docente.Cpf,
            Email = docente.Email,
            Especialidade = docente.Especialidade,
            DataNascimento = (DateOnly)docente.DataNascimento,
            Ativo = true
        };

        await _docenteRepository.AdicionarDocenteAsync(novoDocente);
    }

    public async Task InativarDocenteAsync(int id)
    {
        var docente = await _docenteRepository.ObterPeloIdAsync(id);

        if (docente == null) throw new EntidadeNaoEncontradaException("Erro: O docente a ser inativado não foi encontrado.");

        await _docenteRepository.InativarDocenteAsync(id);
    }
    public async Task ReativarDocenteAsync(int id)
    {
        var docente = await _docenteRepository.ObterInativoPeloIdAsync(id);

        if (docente == null) throw new EntidadeNaoEncontradaException("Erro: O docente a ser reativado não foi encontrado.");

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
        if (docente.DataNascimento >= DateOnly.FromDateTime(DateTime.Today))
            throw new RegraDeNegocioException("A data de nascimento não pode ser maior ou igual a data de atual");

        var docenteExistente = await _docenteRepository.ObterPeloIdAsync(docente.Id);

        if (docenteExistente == null) throw new EntidadeNaoEncontradaException("O docente que você tentou editar não foi encontrado.");

        docenteExistente.Nome = docente.Nome;
        docenteExistente.DataNascimento = docente.DataNascimento;
        docenteExistente.Especialidade = docente.Especialidade;
        docenteExistente.Email = docente.Email;

        await _docenteRepository.EditarDocenteAsync(docenteExistente);
    }
}
