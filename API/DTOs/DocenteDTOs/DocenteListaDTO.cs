namespace API.DTOs.DocenteDTOs;

public record DocenteListaDTO(
    int Id,
    string Nome,
    string? Email,
    string? DisciplinaNome,
    bool Ativo);
