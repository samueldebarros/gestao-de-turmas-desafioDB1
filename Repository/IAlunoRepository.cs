using Common.Domains;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public interface IAlunoRepository
    {
        Aluno ObterPorId(int id);
        void Adicionar(Aluno aluno);
        (List<Aluno> lista, int total) ObterTodosOsAluno(int pagina =1 , int tamanho = 10 , string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null);
        void Excluir(int id);
        void Alterar(Aluno aluno);
        bool ExistePeloCPF(string cpf);
    }
}
