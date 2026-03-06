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

        private async Task<string> ValidarEProcessarCpfAsync(string cpfSujo)
        {
            var cpfLimpo = ValidacaoCpf.Limpar(cpfSujo);

            if (!ValidacaoCpf.IsCpfValido(cpfLimpo)) throw new RegraDeNegocioException("O CPF informado é invalido");

            if (await _alunoRepository.ExistePeloCpfAsync(cpfLimpo)) throw new RegraDeNegocioException("Esse CPF já esta em uso.");

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

        private string GerarMatriculaUnica()
        {
            string prefixo = DateTime.Now.ToString("yyyyMM");
            string aleatorio = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

            string matricula = $"{prefixo}{aleatorio}";

            return matricula;
        }

        public async Task AdicionarAlunoAsync(AlunoInputDTO aluno)
        {
            ValidarDataNascimento(aluno.DataNascimento);

            var cpfLimpo = await ValidarEProcessarCpfAsync(aluno.Cpf);

            if (await _alunoRepository.ExistePeloEmailAsync(aluno.Email))
                throw new RegraDeNegocioException("Este e-mail já esta em uso.");

            Aluno novoAluno = new Aluno
            {
                Nome = aluno.Nome,
                Cpf = cpfLimpo,
                Email = aluno.Email,
                DataNascimento = aluno.DataNascimento,
                Sexo = aluno.Sexo,

                Ativo = true,
                Matricula = GerarMatriculaUnica()
            };

            await _alunoRepository.AdicionarAsync(novoAluno);
        }

        public async Task<ListaPaginada<Aluno>> ObterTodosOsAlunosAsync(int pagina = 1, int tamanho = 10, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null)
        {
            var (alunos, total) = await _alunoRepository.ObterTodosOsAlunoAsync(pagina, tamanho, pesquisa, sexo, ativo);

            var resultadoPaginado = new ListaPaginada<Aluno>(alunos, total, pagina, tamanho);

            return resultadoPaginado;

        }

        public async Task InativarAlunoAsync(int id)
        {
            var alunoExistente = await _alunoRepository.ObterPorIdAsync(id);

            if (alunoExistente == null) throw new EntidadeNaoEncontradaException("O aluno que você tentou inativar não foi encontrado.");
            
            await _alunoRepository.InativarAsync(id);
        }

        public async Task ReativarAlunoAsync(int id)
        {
            var alunoExistente = await _alunoRepository.ObterInativoPorIdAsync(id);

            if (alunoExistente == null) throw new EntidadeNaoEncontradaException("O aluno que você tentou reativar não foi encontrado.");

            await _alunoRepository.ReativarAsync(id);
        }

        public async Task<Aluno> ObterPeloIdAsync(int id)
        {
            return await _alunoRepository.ObterPorIdAsync(id);
        }

        public async Task AlterarAsync(AlterarAlunoDTO aluno)
        {
            var alunoExistente = await _alunoRepository.ObterPorIdAsync(aluno.Id);

            if (alunoExistente == null)
                throw new EntidadeNaoEncontradaException("O aluno que você tentou editar não foi encontrado.");

            if (!alunoExistente.Ativo)
                throw new RegraDeNegocioException("Não é possivel editar um aluno inativo.");

            ValidarDataNascimento(aluno.DataNascimento);

            if (await _alunoRepository.ExistePeloEmailAsync(aluno.Email, aluno.Id))
                throw new RegraDeNegocioException("Este e-mail já esta em uso.");

            alunoExistente.Nome = aluno.Nome;
            alunoExistente.Email = aluno.Email;
            alunoExistente.Sexo = aluno.Sexo;

            if (aluno.DataNascimento.HasValue)
            {
                alunoExistente.DataNascimento = aluno.DataNascimento.Value;
            }
            
            await _alunoRepository.AlterarAsync(alunoExistente);
        }
    }
}
