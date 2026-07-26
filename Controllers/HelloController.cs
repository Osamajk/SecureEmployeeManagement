using Microsoft.AspNetCore.Mvc;

namespace SecureEmployeeManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHello()
    {
        return Ok("Hello from the Secure Employee Management API");
    }

    // ↓↓↓ your new method goes here ↓↓↓
    [HttpGet("{name}")]
    public IActionResult GetHelloByName(string name)
    {
        if (name.Length < 2)
        {
            return BadRequest("...");   // write a real message
        }

        return Ok($"Hello, {name}");
    }
    // ↑↑↑ still inside the class ↑↑↑
}