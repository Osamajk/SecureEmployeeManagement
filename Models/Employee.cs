namespace SecureEmployeeManagement.Models;

/// <summary>
/// Domain entity representing an employee.
/// In Phase 4 this becomes an EF Core entity mapped to a database table.
/// </summary>
public class Employee
{
    public int Id { get; set; }

    // 'required' (C# 11+) forces callers to set this at construction.
    // Combined with <Nullable>enable, the compiler now enforces that
    // FullName can never be null — a compile-time guarantee, not a runtime check.
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Department { get; set; }
    public required string JobTitle { get; set; }

    // UTC everywhere. Storing local time is a classic bug source once
    // you deploy to a cloud region in a different timezone.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }  // '?' = nullable; null until first update
}