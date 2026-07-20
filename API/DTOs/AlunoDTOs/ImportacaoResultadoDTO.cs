namespace API.DTOs.AlunoDTOs;

public record ImportacaoResultadoDTO(int TotalCriado, IReadOnlyList<AlunoCriadoDTO> Criados);
