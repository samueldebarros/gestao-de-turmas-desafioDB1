using API.DTOs;
using API.DTOs.DocenteDTOs;
using Common.Domains;
using Common.Enums;
using Common.Exceptions;
using Common.Utils;
using Repository;

namespace API.Service;

public class DocenteService : IDocenteService
{
    private readonly IDocenteRepository _docenteRepository;
    private readonly IDisciplinaRepository _disciplinaRepository;

    public DocenteService(IDocenteRepository docenteRepository, IDisciplinaRepository disciplinaRepository)
    {
        _docenteRepository = docenteRepository;
        _disciplinaRepository = disciplinaRepository;
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

        var dataMinima18Anos = DateOnly.FromDateTime(DateTime.Today).AddYears(-18);
        if (dataNascimento > dataMinima18Anos)
            throw new RegraDeNegocioException("O docente deve ter pelo menos 18 anos.");

    }

    private async Task<Docente> ObterDocenteAtivoOuLancarErroAsync(int id)
    {
        var docente = await _docenteRepository.ObterPeloIdAsync(id);
        if (docente == null)
            throw new EntidadeNaoEncontradaException("O docente não foi encontrado.");
        return docente;
    }

    private async Task<Docente> ObterDocenteInativoOuLancarErroAsync(int id)
    {
        var docente = await _docenteRepository.ObterInativoPeloIdAsync(id);
        if (docente == null)
            throw new EntidadeNaoEncontradaException("O docente não foi encontrado.");
        return docente;
    }

    private async Task ValidarDisciplinaSeInformadaAsync(int? disciplinaId)
    {
        if (disciplinaId.HasValue)
            if (!await _disciplinaRepository.ExisteAtivaAsync(disciplinaId.Value))
                throw new RegraDeNegocioException("A disciplina informada não existe ou está inativa.");
    }

    private async Task ValidarDadosDocenteAsync(DateOnly? dataNascimento, string? email, int? disciplinaId, int? ignorarId = null)
    {
        ValidarDataNascimento(dataNascimento);
        await ValidarDisciplinaSeInformadaAsync(disciplinaId);

        if (!string.IsNullOrEmpty(email))
            if (await _docenteRepository.ExistePeloEmailAsync(email, ignorarId))
                throw new RegraDeNegocioException("Este e-mail já esta em uso.");
    }

    public async Task AdicionarDocenteAsync(DocenteInputDTO docente)
    {
        await ValidarDadosDocenteAsync(docente.DataNascimento, docente.Email, docente.DisciplinaId);

        var cpfLimpo = await ValidarEProcessarCpfAsync(docente.Cpf);

        Docente novoDocente = new Docente()
        {
            Nome = docente.Nome,
            Cpf = cpfLimpo,
            Email = docente.Email,
            DisciplinaId = docente.DisciplinaId,
            DataNascimento = (DateOnly)docente.DataNascimento,
            Ativo = true
        };

        await _docenteRepository.AdicionarDocenteAsync(novoDocente);
    }

    public async Task InativarDocenteAsync(int id)
    {
        await ObterDocenteAtivoOuLancarErroAsync(id);
        await _docenteRepository.InativarDocenteAsync(id);
    }
    public async Task ReativarDocenteAsync(int id)
    {
        await ObterDocenteInativoOuLancarErroAsync(id);
        await _docenteRepository.ReativarDocenteAsync(id);
    }

    public async Task<ListaPaginada<Docente>> ObterTodosOsDocentesAsync(int pagina = 1, int tamanho = 5, string? pesquisa = null, bool? ativo = null, string? ordenacao = null,
            DirecaoOrdenacaoEnum? direcao = null)
    {
        var (docentes, total) = await _docenteRepository.ObterTodosOsDocentesAsync(pagina, tamanho, pesquisa,ativo, ordenacao, direcao);

        var docentesPaginados = new ListaPaginada<Docente>(docentes, total, pagina, tamanho);

        return docentesPaginados;
    }

    public async Task EditarDocenteAsync(EditarDocenteDTO docente)
    {
        var docenteExistente = await ObterDocenteAtivoOuLancarErroAsync(docente.Id);

        await ValidarDadosDocenteAsync(docente.DataNascimento, docente.Email, docente.DisciplinaId, docente.Id);

        docenteExistente.Nome = docente.Nome;
        docenteExistente.DataNascimento = docente.DataNascimento;
        docenteExistente.DisciplinaId = docente.DisciplinaId;
        docenteExistente.Email = docente.Email;

        await _docenteRepository.EditarDocenteAsync(docenteExistente);
    }
    public async Task<Docente> ObterPeloIdAsync(int id)
    {
        return await _docenteRepository.ObterPeloIdAsync(id);
    }
}
