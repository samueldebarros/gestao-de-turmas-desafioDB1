using API.DTOs.AlunoDTOs;

namespace API.Exceptions;

public class ImportacaoInvalidaException : Exception
{
    public IReadOnlyList<LinhaErroDTO> Erros { get; }

    public ImportacaoInvalidaException(IReadOnlyList<LinhaErroDTO> erros)
        : base("O lote de importação contém linhas inválidas.")
    {
        Erros = erros;
    }
}
