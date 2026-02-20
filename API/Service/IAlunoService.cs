using API.DTOs;
using Common.Domains;
using Common.Enums;
using Repository;

namespace API.Service
{
    public interface IAlunoService
    {
        void AdicionarAluno(AlunoInputDTO aluno);
        ListaPaginada<Aluno> ObterTodosOsAlunos(int pagina = 1, int tamanho = 10, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null);
        Aluno ObterPeloId(int id);
        void ExcluirAluno(int id);
        void Alterar(AlterarAlunoDTO aluno);
    }
}
