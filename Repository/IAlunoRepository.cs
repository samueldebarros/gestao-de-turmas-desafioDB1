using Common.Domains;
using Common.Enums;

namespace Repository
{
    public interface IAlunoRepository
    {
        Task<Aluno> ObterPorIdAsync(int id);
        Task<Aluno> ObterInativoPorIdAsync(int id);
        Task AdicionarAsync(Aluno aluno);
        Task<(List<Aluno> lista, int total)> ObterTodosOsAlunoAsync(int pagina =1 , int tamanho = 10 , string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null);
        Task InativarAsync(int id);
        Task ReativarAsync(int id);
        Task AlterarAsync(Aluno aluno);
        Task<bool> ExistePeloCPFAsync(string cpf);
    }
}
