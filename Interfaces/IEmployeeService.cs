using SecureEmployeeManagement.DTOs;

namespace SecureEmployeeManagement.Interfaces;

/// <summary>
/// The contract. The controller depends on THIS, never on a concrete class.
/// Why: in Phase 4 we swap the in-memory implementation for an EF Core one
/// and the controller doesn't change at all. Also makes unit testing trivial —
/// you mock the interface instead of standing up a database.
/// C# convention: interfaces are prefixed with 'I'.
/// </summary>
public interface IEmployeeService
{
    IEnumerable<EmployeeResponseDto> GetAll();
    EmployeeResponseDto? GetById(int id);          // '?' → null when not found
    EmployeeResponseDto Create(CreateEmployeeDto dto);
    EmployeeResponseDto? Update(int id, UpdateEmployeeDto dto);
    bool Delete(int id);                            // false when not found
}