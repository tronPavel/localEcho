using System.ComponentModel.DataAnnotations;

namespace LocalEcho.Application.Dtos;

public record ChangeDistrictDto(
    [Required] Guid DistrictId,
    string? HomeAddress
);