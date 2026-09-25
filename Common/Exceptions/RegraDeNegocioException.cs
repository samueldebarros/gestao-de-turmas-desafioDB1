namespace Common.Exceptions;

public class RegraDeNegocioException : Exception
{
    public string? Codigo { get; }
    public IReadOnlyDictionary<string, object>? Params { get; }

    public RegraDeNegocioException(string message) : base(message) { }

    public RegraDeNegocioException(string codigo, string message, IReadOnlyDictionary<string, object>? parametros = null)
        : base(message)
    {
        Codigo = codigo;
        Params = parametros;
    }
}
