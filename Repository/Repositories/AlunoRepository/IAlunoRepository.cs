using Common.Domains;
using Common.Enums;
using Repository.Repositories.Base;

namespace Repository.Repositories
{
    public interface IAlunoRepository : IBaseInativavelRepository<Aluno>
    {
        Task<List<Aluno>> ObterAlunosDisponiveisParaTurmaAsync(int turmaId);
        Task<(List<Aluno> lista, int total)> ObterTodosOsAlunoAsync(int pagina =1 , int tamanho = 10 , string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null,
            string? ordenacao = null, DirecaoOrdenacaoEnum? direcao = null);
        Task<bool> ExistePeloCpfAsync(string cpf);
        Task<bool> ExistePeloEmailAsync(string email, int? ignorarId = null);
        Task<bool> ExisteMatriculaAsync(string matricula);
    }
}
