using System.ComponentModel.DataAnnotations;

namespace SecureEmployeeManagement.DTOs;

public record RegisterDto(
    [Required][StringLength(100, MinimumLength = 3)] string Username,
    [Required][EmailAddress][StringLength(256)] string Email,
    [Required][StringLength(100, MinimumLength = 8)] string Password
);
// NOTE: no Role field. A user must NOT choose their own role at registration —
// that would be privilege escalation (mass assignment). Role is server-assigned.

public record LoginDto(
    [Required] string Username,
    [Required] string Password
);

public record AuthResponseDto(
    string Token,
    string Username,
    string Role
);