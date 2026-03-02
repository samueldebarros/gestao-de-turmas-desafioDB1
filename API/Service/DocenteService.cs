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
        if (await _docenteRepository.ExistePeloCpfAsync(docente.Cpf)) throw new RegraDeNegocioException("Este CPF já esta em uso.");

        Docente novoDocente = new Docente()
        {
            Nome = docente.Nome,
            Cpf = docente.Cpf,
            Email = docente.Email,
            Especialidade = docente.Especialidade,
            DataNascimento = docente.DataNascimento,
            Ativo = true
        };

        await _docenteRepository.AdicionarDocenteAsync(novoDocente);
    }

    public async Task InativarDocenteAsync(int id)
    {
        var docente = _docenteRepository.ObterPeloIdAsync(id);

        if (docente == null) throw new EntidadeNaoEncontradaException("Erro: O docente a ser inativado não foi encontrado.");

        await _docenteRepository.InativarDocenteAsync(id);
    }
    public async Task ReativarDocenteAsync(int id)
    {
        var docente = _docenteRepository.ObterPeloIdAsync(id);

        if (docente == null) throw new EntidadeNaoEncontradaException("Erro: O docente a ser inativado não foi encontrado.");

        await _docenteRepository.ReativarDocenteAsync(id);
    }

    public async Task<List<Docente>> ObterTodosOsDocentesAsync()
    {
        return await _docenteRepository.ObterTodosOsDocentesAsync();
    }

    public async Task EditarDocenteAsync(EditarDocenteDTO docente)
    {
        var docenteExistente = await _docenteRepository.ObterPeloIdAsync(docente.Id);

        if (docenteExistente == null) throw new EntidadeNaoEncontradaException("O aluno que você tentou editar não foi encontrado.");

        docenteExistente.Id = docente.Id;
        docenteExistente.Nome = docente.Nome;
        docenteExistente.DataNascimento = docente.DataNascimento;
        docenteExistente.Especialidade = docente.Especialidade;
        docenteExistente.Email = docente.Email;

        await _docenteRepository.EditarDocenteAsync(docenteExistente);
    }
}
