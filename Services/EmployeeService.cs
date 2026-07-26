using Microsoft.EntityFrameworkCore;
using SecureEmployeeManagement.Data;
using SecureEmployeeManagement.DTOs;
using SecureEmployeeManagement.Exceptions;
using SecureEmployeeManagement.Interfaces;
using SecureEmployeeManagement.Models;

namespace SecureEmployeeManagement.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context) => _context = context;

    public IEnumerable<EmployeeResponseDto> GetAll()
        => _context.Employees.AsNoTracking().Select(MapToDto).ToList();

    public EmployeeResponseDto? GetById(int id)
    {
        var e = _context.Employees.AsNoTracking().FirstOrDefault(x => x.Id == id);
        return e is null ? null : MapToDto(e);
    }

    public EmployeeResponseDto Create(CreateEmployeeDto dto)
    {
        // Application-level check so we can return a meaningful 409
        // instead of letting a raw SqlException bubble up as a 500.
        // The DB unique index remains as the hard guarantee (defense in depth).
        if (_context.Employees.Any(e => e.Email == dto.Email))
            throw new DuplicateEmailException(dto.Email);

        var employee = new Employee
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Department = dto.Department,
            JobTitle = dto.JobTitle,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        _context.SaveChanges();
        return MapToDto(employee);
    }

    public EmployeeResponseDto? Update(int id, UpdateEmployeeDto dto)
    {
        var employee = _context.Employees.FirstOrDefault(x => x.Id == id);
        if (employee is null) return null;

        // Is another employee already using this email?
        if (_context.Employees.Any(e => e.Email == dto.Email && e.Id != id))
            throw new DuplicateEmailException(dto.Email);

        employee.FullName = dto.FullName;
        employee.Email = dto.Email;
        employee.Department = dto.Department;
        employee.JobTitle = dto.JobTitle;
        employee.UpdatedAt = DateTime.UtcNow;

        _context.SaveChanges();
        return MapToDto(employee);
    }

    public bool Delete(int id)
    {
        var employee = _context.Employees.FirstOrDefault(x => x.Id == id);
        if (employee is null) return false;

        _context.Employees.Remove(employee);
        _context.SaveChanges();
        return true;
    }

    private static EmployeeResponseDto MapToDto(Employee e)
        => new(e.Id, e.FullName, e.Email, e.Department, e.JobTitle, e.CreatedAt, e.UpdatedAt);
}