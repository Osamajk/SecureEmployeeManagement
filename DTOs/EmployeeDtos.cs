using System.ComponentModel.DataAnnotations;

namespace SecureEmployeeManagement.DTOs;

// DataAnnotations = declarative validation rules.
// Because controllers have [ApiController], ASP.NET Core automatically
// returns 400 with a structured error list BEFORE your method body runs.
// This is your first line of defence: bad input never reaches business logic.
public record CreateEmployeeDto(
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, MinimumLength = 2)]
    string FullName,

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256)]
    string Email,

    [Required]
    [StringLength(100, MinimumLength = 2)]
    string Department,

    [Required]
    [StringLength(100, MinimumLength = 2)]
    string JobTitle
);

public record UpdateEmployeeDto(
    [Required][StringLength(200, MinimumLength = 2)] string FullName,
    [Required][EmailAddress][StringLength(256)] string Email,
    [Required][StringLength(100, MinimumLength = 2)] string Department,
    [Required][StringLength(100, MinimumLength = 2)] string JobTitle
);

public record EmployeeResponseDto(
    int Id, string FullName, string Email,
    string Department, string JobTitle,
    DateTime CreatedAt, DateTime? UpdatedAt
);