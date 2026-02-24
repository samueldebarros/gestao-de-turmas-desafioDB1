using Common.Domains;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public interface IAlunoRepository
    {
        Task<Aluno> ObterPorIdAsync(int id);
        Task AdicionarAsync(Aluno aluno);
        Task<(List<Aluno> lista, int total)> ObterTodosOsAlunoAsync(int pagina =1 , int tamanho = 10 , string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null);
        Task ExcluirAsync(int id);
        Task AlterarAsync(Aluno aluno);
        bool ExistePeloCPF(string cpf);
    }
}
