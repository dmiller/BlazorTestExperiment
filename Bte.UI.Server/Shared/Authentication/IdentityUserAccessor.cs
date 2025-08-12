using Bte.Core;
using Microsoft.AspNetCore.Identity;

namespace Bte.UI.Server.Shared.Authentication;

// The methods in these classes should only be used in SSR (non-interactive) situations.
// They do not use a DbContextFactory, so we end up with scoping/lifetime issues in interactive scenarios.

internal sealed class IdentityUserAccessor(UserManager<ApplicationUser> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        }

        return user;
    }

    public async Task<ApplicationUser> FindUserAsync(HttpContext context, Guid guid)
    {
        try
        {


            var user = await userManager.FindByIdAsync(guid.ToString());
            if (user is null)
            {
                redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{guid}'.", context);
            }
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Silent error: " + ex.Message);
            throw;
        }
    }
}