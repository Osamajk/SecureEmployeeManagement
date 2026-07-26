using SecureEmployeeManagement.DTOs;

namespace SecureEmployeeManagement.Interfaces;

public interface IAuthService
{
    AuthResponseDto? Register(RegisterDto dto);   // null if username/email taken
    AuthResponseDto? Login(LoginDto dto);         // null if credentials invalid
}