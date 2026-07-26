using SecureEmployeeManagement.Models;

namespace SecureEmployeeManagement.Data;

public static class DbSeeder
{
    public static void SeedAdmin(AppDbContext context, IConfiguration config)
    {
        if (context.Users.Any(u => u.Role == "Admin"))
            return;

        var adminPassword = config["Seed:AdminPassword"] ?? "ChangeMe_Admin123";

        context.Users.Add(new User
        {
            Username = "admin",
            Email = "admin@securemployee.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = "Admin"
        });
        context.SaveChanges();
    }
}