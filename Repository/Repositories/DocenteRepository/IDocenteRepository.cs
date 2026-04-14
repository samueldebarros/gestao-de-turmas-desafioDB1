using Common.Domains;
using Common.Enums;
using Repository.Repositories.Base;

namespace Repository.Repositories.DocenteRepository;

public interface IDocenteRepository : IBaseInativavelRepository<Docente>
{
    Task<List<Docente>> ObterDocentesPorDisciplinaAsync(int disciplinaId);
    Task<(List<Docente>, int total)> ObterTodosOsDocentesAsync(int pagina = 1, int tamanho = 5, string? pesquisa = null, bool? ativo = null, string? ordenacao = null,
            DirecaoOrdenacaoEnum? direcao = null);
    Task<bool> ExistePeloCpfAsync(string cpf);
    Task<bool> ExistePeloEmailAsync(string email, int? ignorarId = null);
}
