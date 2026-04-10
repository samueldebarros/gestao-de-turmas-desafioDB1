using API.DTOs;
using API.DTOs.AlunoDTOs;
using Common.Domains;
using Common.Enums;
using Common.Exceptions;
using Common.Utils;
using Repository;

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

        private async Task<string> GerarMatriculaUnicaAsync()
        {
            string prefixo = DateTime.Now.ToString("yyyyMM");
            for (int tentativa = 0; tentativa < 5; tentativa++)
            {
                string aleatorio = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                string matricula = $"{prefixo}{aleatorio}";

                if (!await _alunoRepository.ExisteMatriculaAsync(matricula))
                    return matricula;
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

        public async Task AdicionarAlunoAsync(AlunoInputDTO aluno)
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
        }

        public async Task<ListaPaginada<Aluno>> ObterTodosOsAlunosAsync(int pagina = 1, int tamanho = 10, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null,
            string? ordenacao = null,
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
            
            await _alunoRepository.EditarAlunoAsync(alunoExistente);
        }
    }
}
