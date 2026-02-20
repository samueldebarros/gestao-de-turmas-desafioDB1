using API.DTOs;
using Common.Domains;
using Common.Enums;
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

        private string GerarMatriculaUnica()
        {
            string prefixo = DateTime.Now.ToString("yyyyMM");
            string aleatorio = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

            string matricula = $"{prefixo}{aleatorio}";

            return matricula;
        }

        public void AdicionarAluno(AlunoInputDTO aluno)
        {

            if (_alunoRepository.ExistePeloCPF(aluno.Cpf))
            {
                throw new InvalidOperationException("Esse CPF já esta em uso");
            }

            Aluno novoAluno = new Aluno
            {
                Nome = aluno.Nome,
                Cpf = aluno.Cpf,
                Email = aluno.Email,
                DataNascimento = aluno.DataNascimento,
                Sexo = aluno.Sexo,

                Ativo = true,
                Matricula = GerarMatriculaUnica()
            };

            _alunoRepository.Adicionar(novoAluno);
        }

        public ListaPaginada<Aluno> ObterTodosOsAlunos(int pagina = 1, int tamanho = 10, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null)
        {
            var (alunos, total) = _alunoRepository.ObterTodosOsAluno(pagina, tamanho, pesquisa, sexo, ativo);

            var resultadoPaginado = new ListaPaginada<Aluno>(alunos, total, pagina, tamanho);

            return resultadoPaginado;

        }

        public void ExcluirAluno(int id)
        {
            var alunoExistente = _alunoRepository.ObterPorId(id);

            if (alunoExistente == null)
            {
                throw new KeyNotFoundException("O aluno que você tentou excluir não foi encontrado.");
            }

            _alunoRepository.Excluir(id);
        }

        public Aluno ObterPeloId(int id)
        {
            return _alunoRepository.ObterPorId(id);
        }

        public void Alterar(AlterarAlunoDTO aluno)
        {

            var alunoExistente = _alunoRepository.ObterPorId(aluno.Id);

            if (alunoExistente == null)
            {
                throw new KeyNotFoundException("O aluno que você tentou editar não existe.");
            }

            alunoExistente.Nome = aluno.Nome;
            alunoExistente.Email = aluno.Email;
            alunoExistente.Sexo = aluno.Sexo;

            if (aluno.DataNascimento.HasValue)
            {
                alunoExistente.DataNascimento = aluno.DataNascimento.Value;
            }
            
            _alunoRepository.Alterar(alunoExistente);


        }
    }
}
