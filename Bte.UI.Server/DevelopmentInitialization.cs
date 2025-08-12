
using Bte.Application;
using Bte.Core;
using Microsoft.AspNetCore.Identity;

namespace Bte.UI.Server;

public static class DevelopmentInitialization
{
    // To be called from Program.cs only in develoment mode to make sure we have standard users and roles created for hands-on testing.
    public static async Task EnsureUsers(IServiceProvider serviceProvider)
    {
        using var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var authService = serviceProvider.GetRequiredService<IUserService>();

        await CreateNewUser(userManager, authService, "superuser@abc.com", "Asdf1234!", "Joe", "Super", "Superuser");
        await CreateNewUser(userManager, authService, "admin@abc.com", "Asdf1234!", "Joe", "Admin", "Admin");
        await CreateNewUser(userManager, authService, "u1@abc.com", "Asdf1234!", "Joe", "Manager", "User");
        await CreateNewUser(userManager, authService, "u2@abc.com", "Asdf1234!", "Joe", "Scheduler", "User");

    }

    private static async Task CreateNewUser(
        UserManager<ApplicationUser> userManager,
        IUserService authenticationService,
        string email,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is null)
        {
            var response = await authenticationService.RegisterUserAsync(email, password, firstName, lastName);
            if (response.IsFailure)
            {
                throw new Exception($"Failed to create {email} account.");
            }
            var newUser = await userManager.FindByIdAsync(response.Value.ToString()) ?? throw new Exception($"Failed to find newly created {email} account.");
            await userManager.AddToRoleAsync(newUser, role.ToString());
        }
    }
}
