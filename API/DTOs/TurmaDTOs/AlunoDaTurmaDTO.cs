using Common.Enums;

namespace API.DTOs.TurmaDTOs;

public record AlunoDaTurmaDTO(
    int Id,
    string Matricula,
    string Nome,
    string Cpf,
    string? Email,
    SexoEnum Sexo,
    DateOnly DataNascimento);
