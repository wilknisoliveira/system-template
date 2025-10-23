using Auth.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Context;

public abstract class DatabaseSeeder
{
    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = typeof(Roles)
            .GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
            )
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .ToArray();

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task EnsureUsersAsync(UserManager<IdentityUser> userManager)
    {
        const string adminEmail = "admin@systemtemplate.com";
        // Please change the admin password in the first system initialization
        const string password = "systemTemplate#123";

        if (await userManager.FindByEmailAsync(adminEmail) != null)
            return;

        var adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(adminUser, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        else
            throw new Exception(
                "Something went wrong while trying to create the default admin user"
            );
    }

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await EnsureRolesAsync(roleManager);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        await EnsureUsersAsync(userManager);
    }
}
