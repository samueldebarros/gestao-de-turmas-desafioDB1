namespace API.DTOs;

public record ErroNegocioDTO(string? Codigo, IReadOnlyDictionary<string, object>? Params, string Mensagem);
