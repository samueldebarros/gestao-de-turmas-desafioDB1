using API.DTOs;
using API.DTOs.AlunoDTOs;
using Common.Domains;
using Common.Enums;

namespace API.Service
{
    public interface IAlunoService
    {
        Task AdicionarAlunoAsync(AlunoInputDTO aluno);
        Task<ListaPaginada<Aluno>> ObterTodosOsAlunosAsync(int pagina = 1, int tamanho = 10, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null, string? ordenacao = null,
            DirecaoOrdenacaoEnum? direcao = null);
        Task<Aluno> ObterPeloIdAsync(int id);
        Task InativarAlunoAsync(int id);
        Task ReativarAlunoAsync(int id);
        Task AlterarAsync(AlterarAlunoDTO aluno);
    }
}
