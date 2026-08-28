namespace API.DTOs.DocenteDTOs;

public record DocenteDetalheDTO(
    int Id,
    string Nome,
    string? Email,
    DateOnly DataNascimento,
    int? DisciplinaId,
    bool Ativo);
