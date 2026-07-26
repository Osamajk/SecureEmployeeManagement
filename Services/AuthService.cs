using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SecureEmployeeManagement.Data;
using SecureEmployeeManagement.DTOs;
using SecureEmployeeManagement.Interfaces;
using SecureEmployeeManagement.Models;

namespace SecureEmployeeManagement.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public AuthResponseDto? Register(RegisterDto dto)
    {
        // Reject duplicates up front.
        if (_context.Users.Any(u => u.Username == dto.Username || u.Email == dto.Email))
            return null;

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            // BCrypt.HashPassword generates a random salt and embeds it in the
            // hash string. Same password -> different hash every time. This
            // defeats rainbow tables. BCrypt is deliberately SLOW, which makes
            // brute-forcing expensive even on GPUs.
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Employee"   // SERVER decides the role, never the client
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return new AuthResponseDto(GenerateToken(user), user.Username, user.Role);
    }

    public AuthResponseDto? Login(LoginDto dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == dto.Username);

        // IMPORTANT: same generic failure whether the username doesn't exist OR
        // the password is wrong. Distinct errors would let an attacker enumerate
        // valid usernames (an authentication failure / info-leak issue).
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        return new AuthResponseDto(GenerateToken(user), user.Username, user.Role);
    }

    private string GenerateToken(User user)
    {
        var jwtKey = _config["Jwt:Key"]!;
        var jwtIssuer = _config["Jwt:Issuer"]!;
        var jwtAudience = _config["Jwt:Audience"]!;

        // Claims = statements about the user embedded in the token.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),   // drives [Authorize(Roles=...)]
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // The token is SIGNED with our secret key. Any tampering (e.g. changing
        // the role claim to Admin) invalidates the signature -> rejected.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),   // short-lived tokens limit exposure
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}