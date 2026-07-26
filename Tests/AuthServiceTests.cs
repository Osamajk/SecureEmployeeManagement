using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SecureEmployeeManagement.Data;
using SecureEmployeeManagement.DTOs;
using SecureEmployeeManagement.Services;
using Xunit;

namespace Tests;

public class AuthServiceTests
{
    // Helper: a fresh in-memory database per test (isolated, no shared state).
    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // Helper: minimal config providing the JWT settings the service needs.
    private static IConfiguration NewConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestOnlySecretKeyThatIsAtLeast32CharsLong123",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();
    }

    [Fact]  // [Fact] marks a single test method
    public void Register_NewUser_ReturnsTokenAndEmployeeRole()
    {
        var service = new AuthService(NewDb(), NewConfig());

        var result = service.Register(new RegisterDto("alice", "alice@test.com", "password123"));

        Assert.NotNull(result);
        Assert.Equal("alice", result!.Username);
        Assert.Equal("Employee", result.Role);      // server assigns role, never client
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public void Register_DuplicateUsername_ReturnsNull()
    {
        var db = NewDb();
        var service = new AuthService(db, NewConfig());

        service.Register(new RegisterDto("bob", "bob@test.com", "password123"));
        var second = service.Register(new RegisterDto("bob", "other@test.com", "password123"));

        Assert.Null(second);   // duplicate rejected
    }

    [Fact]
    public void Register_ThenLogin_CorrectPassword_Succeeds()
    {
        var db = NewDb();
        var service = new AuthService(db, NewConfig());
        service.Register(new RegisterDto("carol", "carol@test.com", "password123"));

        var login = service.Login(new LoginDto("carol", "password123"));

        Assert.NotNull(login);
        Assert.False(string.IsNullOrEmpty(login!.Token));
    }

    [Fact]
    public void Login_WrongPassword_ReturnsNull()
    {
        var db = NewDb();
        var service = new AuthService(db, NewConfig());
        service.Register(new RegisterDto("dave", "dave@test.com", "password123"));

        var login = service.Login(new LoginDto("dave", "wrongpassword"));

        Assert.Null(login);   // wrong password → no token
    }

    [Fact]
    public void Password_IsHashed_NotStoredPlaintext()
    {
        var db = NewDb();
        var service = new AuthService(db, NewConfig());

        service.Register(new RegisterDto("erin", "erin@test.com", "password123"));

        var stored = db.Users.Single();
        Assert.NotEqual("password123", stored.PasswordHash);   // never plaintext
        Assert.StartsWith("$2", stored.PasswordHash);          // BCrypt hash prefix
    }
}