using API.DTOs;
using API.DTOs.AlunoDTOs;
using API.Exceptions;
using Common.Domains;
using Common.Enums;
using Common.Exceptions;
using Common.Utils;
using Repository.Repositories;
using System.ComponentModel.DataAnnotations;

namespace API.Service
{
    public class AlunoService : IAlunoService
    {
        private readonly IAlunoRepository _alunoRepository;

        public AlunoService(IAlunoRepository alunoRepository)
        {
            _alunoRepository = alunoRepository;
        }

        private void ValidarDataNascimento(DateOnly? dataNascimento)
        {
            if (dataNascimento >= DateOnly.FromDateTime(DateTime.Today))
                throw new RegraDeNegocioException("A data de nascimento não pode ser maior ou igual à data atual.");

            var dataMinimaAceitavel = DateOnly.FromDateTime(DateTime.Today).AddYears(-120);
            if (dataNascimento < dataMinimaAceitavel)
                throw new RegraDeNegocioException("A data de nascimento informada é inválida (idade superior a 120 anos).");
        }

        private Task<string> GerarMatriculaUnicaAsync() =>
            GerarMatriculaUnicaAsync(new HashSet<string>());

        private async Task<string> GerarMatriculaUnicaAsync(ISet<string> usadasNoLote)
        {
            string prefixo = DateTime.Now.ToString("yyyyMM");
            for (int tentativa = 0; tentativa < 5; tentativa++)
            {
                string aleatorio = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                string matricula = $"{prefixo}{aleatorio}";

                if (usadasNoLote.Contains(matricula))
                    continue;

                if (!await _alunoRepository.ExisteMatriculaAsync(matricula))
                {
                    usadasNoLote.Add(matricula);
                    return matricula;
                }
            }
            throw new RegraDeNegocioException("Não foi possível gerar uma matrícula única. Tente novamente!!");
        }

        private async Task<Aluno> ObterAlunoAtivoOuLancarErroAsync(int id)
        {
            var aluno = await _alunoRepository.ObterPorIdAsync(id);
            if (aluno == null)
                throw new EntidadeNaoEncontradaException("O aluno não foi encontrado.");
            return aluno;
        }

        private async Task<Aluno> ObterAlunoInativoOuLancarErroAsync(int id)
        {
            var aluno = await _alunoRepository.ObterInativoPorIdAsync(id);
            if (aluno == null)
                throw new EntidadeNaoEncontradaException("O aluno não foi encontrado.");
            return aluno;
        }

        private async Task ValidarDadosAlunoAsync(DateOnly? dataNascimento, string? email, SexoEnum? sexo, int? ignorarId = null)
        {
            ValidarDataNascimento(dataNascimento);

            if (!string.IsNullOrEmpty(email))
                if (await _alunoRepository.ExistePeloEmailAsync(email, ignorarId))
                    throw new RegraDeNegocioException("Este e-mail já esta em uso.");

            if (!sexo.HasValue)
                throw new RegraDeNegocioException("Selecione um sexo.");

        }

        private LinhaErroDTO? ValidarLinha(
            AlunoInputDTO a, int indice,
            ISet<string> cpfsNoLote, ISet<string> emailsNoLote,
            ISet<string> cpfsNoBanco, ISet<string> emailsNoBanco)
        {
            if (string.IsNullOrWhiteSpace(a.Nome) || a.Nome.Trim().Length is < 3 or > 100)
                return new(indice, nameof(a.Nome), "NOME_INVALIDO");

            if (!Enum.IsDefined(a.Sexo))
                return new(indice, nameof(a.Sexo), "SEXO_INVALIDO");

            var hoje = DateOnly.FromDateTime(DateTime.Today);
            if (a.DataNascimento >= hoje || a.DataNascimento < hoje.AddYears(-120))
                return new(indice, nameof(a.DataNascimento), "DATA_INVALIDA");

            if (!string.IsNullOrWhiteSpace(a.Email))
            {
                if (!EmailBemFormado(a.Email)) 
                    return new(indice, nameof(a.Email), "EMAIL_INVALIDO");
                var email = a.Email.Trim().ToLowerInvariant();
                if (emailsNoBanco.Contains(email))
                    return new(indice, nameof(a.Email), "EMAIL_JA_EXISTE");
                if (!emailsNoLote.Add(email))
                    return new(indice, nameof(a.Email), "EMAIL_DUPLICADO_NO_LOTE");
            }

            var cpf = ValidacaoCpf.Limpar(a.Cpf);
            if (!ValidacaoCpf.IsCpfValido(cpf)) 
                return new(indice, nameof(a.Cpf), "CPF_INVALIDO");
            if (cpfsNoBanco.Contains(cpf))
                return new(indice, nameof(a.Cpf), "CPF_JA_EXISTE");
            if (!cpfsNoLote.Add(cpf))
                return new(indice, nameof(a.Cpf), "CPF_DUPLICADO_NO_LOTE");

            return null;
        }
        private static bool EmailBemFormado(string email) =>
           new EmailAddressAttribute().IsValid(email);

        public async Task<Aluno> AdicionarAlunoAsync(AlunoInputDTO aluno)
        {
            await ValidarDadosAlunoAsync(aluno.DataNascimento, aluno.Email, aluno.Sexo);

            var cpfLimpo = await ValidacaoCpf.ValidarEProcessarCpfAsync(aluno.Cpf, _alunoRepository.ExistePeloCpfAsync);

            Aluno novoAluno = new Aluno
            {
                Nome = aluno.Nome,
                Cpf = cpfLimpo,
                Email = aluno.Email,
                DataNascimento = aluno.DataNascimento,
                Sexo = aluno.Sexo,

                Ativo = true,
                Matricula = await GerarMatriculaUnicaAsync()
            };

            await _alunoRepository.AdicionarAsync(novoAluno);
            return novoAluno;
        }

        public async Task<ImportacaoResultadoDTO> ImportarAlunosAsync(ImportarAlunosRequest request)
        {
            var itens = request.Alunos;

            var cpfsBanco = await _alunoRepository.ObterCpfsExistentesAsync(
                itens.Select(i => ValidacaoCpf.Limpar(i.Cpf)));
            var emailsBanco = await _alunoRepository.ObterEmailsExistentesAsync(
                itens.Where(i => !string.IsNullOrWhiteSpace(i.Email))
                     .Select(i => i.Email!));

            var erros = new List<LinhaErroDTO>();
            var cpfsLote = new HashSet<string>();
            var emailsLote = new HashSet<string>();
            for (int i = 0; i < itens.Count; i++)
            {
                var erro = ValidarLinha(itens[i], i, cpfsLote, emailsLote, cpfsBanco, emailsBanco);
                if (erro is not null)
                    erros.Add(erro);
            }
            if (erros.Count > 0)
                throw new ImportacaoInvalidaException(erros);

            var matriculasUsadas = new HashSet<string>();
            var alunos = new List<Aluno>(itens.Count);
            foreach (var item in itens)
            {
                alunos.Add(new Aluno
                {
                    Nome = item.Nome.Trim(),
                    Cpf = ValidacaoCpf.Limpar(item.Cpf),
                    Email = item.Email?.Trim(),
                    DataNascimento = item.DataNascimento,
                    Sexo = item.Sexo,
                    Ativo = true,
                    Matricula = await GerarMatriculaUnicaAsync(matriculasUsadas)
                });
            }

            await _alunoRepository.AdicionarVariosAsync(alunos);

            var criados = alunos
                .Select(a => new AlunoCriadoDTO(a.Id, a.Matricula, a.Cpf))
                .ToList();
            return new ImportacaoResultadoDTO(criados.Count, criados);
        }

        public async Task<ListaPaginada<Aluno>> ObterTodosOsAlunosAsync(int pagina = 1, int tamanho = 10, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null,
            OrdenacaoAlunoEnum? ordenacao = null,
            DirecaoOrdenacaoEnum? direcao = null)
        {
            var (alunos, total) = await _alunoRepository.ObterTodosOsAlunoAsync(pagina, tamanho, pesquisa, sexo, ativo, ordenacao, direcao);

            var resultadoPaginado = new ListaPaginada<Aluno>(alunos, total, pagina, tamanho);

            return resultadoPaginado;

        }

        public async Task InativarAlunoAsync(int id)
        {
            await ObterAlunoAtivoOuLancarErroAsync(id);
            await _alunoRepository.InativarAsync(id);
        }

        public async Task ReativarAlunoAsync(int id)
        {
            await ObterAlunoInativoOuLancarErroAsync(id);
            await _alunoRepository.ReativarAsync(id);
        }

        public async Task<Aluno> ObterPeloIdAsync(int id)
        {
            return await _alunoRepository.ObterPorIdAsync(id);
        }

        public async Task EditarAlunoAsync(EditarAlunoDTO aluno)
        {
            var alunoExistente = await ObterAlunoAtivoOuLancarErroAsync(aluno.Id);
            
            await ValidarDadosAlunoAsync(aluno.DataNascimento, aluno.Email, aluno.Sexo, aluno.Id);

            alunoExistente.Nome = aluno.Nome;
            alunoExistente.Email = aluno.Email;
            alunoExistente.Sexo = aluno.Sexo;

            if (aluno.DataNascimento.HasValue)
            {
                alunoExistente.DataNascimento = aluno.DataNascimento.Value;
            }
            
            await _alunoRepository.EditarAsync(alunoExistente);
        }


    }
}
