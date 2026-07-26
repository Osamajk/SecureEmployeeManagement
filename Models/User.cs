namespace SecureEmployeeManagement.Models;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }

    // NEVER a plaintext password. This stores a BCrypt hash only.
    // Even a full DB breach doesn't reveal the original passwords.
    public required string PasswordHash { get; set; }

    // "Admin" or "Employee". Drives role-based authorization in Phase 7.
    public required string Role { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}