using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureEmployeeManagement.DTOs;
using SecureEmployeeManagement.Exceptions;
using SecureEmployeeManagement.Interfaces;

namespace SecureEmployeeManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]   // class-level: every endpoint requires a valid token (401 without one)
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    // READ endpoints: any authenticated user (Admin OR Employee).
    // Only the class-level [Authorize] applies here.

    [HttpGet]
    public IActionResult GetAll()
        => Ok(_employeeService.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var employee = _employeeService.GetById(id);
        if (employee is null)
            return NotFound();

        return Ok(employee);
    }

    // WRITE endpoints: Admin role required.
    // A valid Employee token here → 403 Forbidden (authenticated, not permitted).
    // No token at all → 401 Unauthorized.

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Create([FromBody] CreateEmployeeDto dto)
    {
        try
        {
            var created = _employeeService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (DuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        try
        {
            var updated = _employeeService.Update(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        var deleted = _employeeService.Delete(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}