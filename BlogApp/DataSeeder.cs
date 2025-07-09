using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

public static class DataSeeder
{
    private static readonly string[] Roles = new[] { "Admin", "Moderator", "User" };

    public static async Task SeedRolesAndUsersAsync(RoleManager<Role> roleManager, UserManager<ApplicationUser> userManager)
    {
        // Создаем роли
        foreach (var roleName in Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role { Name = roleName });
            }
        }

        // Создаем админа
        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Создаем модератора
        var modEmail = "moderator@example.com";
        var modUser = await userManager.FindByEmailAsync(modEmail);
        if (modUser == null)
        {
            modUser = new ApplicationUser
            {
                UserName = "moderator",
                Email = modEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(modUser, "Moderator123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(modUser, "Moderator");
            }
        }

        // Создаем обычного пользователя
        var userEmail = "user@example.com";
        var normalUser = await userManager.FindByEmailAsync(userEmail);
        if (normalUser == null)
        {
            normalUser = new ApplicationUser
            {
                UserName = "user",
                Email = userEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(normalUser, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(normalUser, "User");
            }
        }
    }
}