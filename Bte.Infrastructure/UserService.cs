using Bte.Application;
using Bte.Core;
using Bte.MediatR;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bte.Infrastructure;

internal class UserService(
    AuthenticationStateProvider authProvider,
    SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
    IUserStore<ApplicationUser> userStore,
    IDbContextFactory<ApplicationDbContext> dbContextFactory) : IUserService
{

    public async Task<ESignInResult> LoginUserAsync(string username, string password, bool rememberMe)
    {
        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        var result = await signInManager.PasswordSignInAsync(username, password, rememberMe, lockoutOnFailure: false);
        ESignInResult eResult = ToESignInResult(result);
        return eResult;
    }

    private static ESignInResult ToESignInResult(SignInResult result)
    {
        if (result.IsNotAllowed)
            return ESignInResult.IsNotAllowed;
        else if (result.IsLockedOut)
            return ESignInResult.IsLockedOut;
        else if (result.IsLockedOut)
            return ESignInResult.IsLockedOut;
        else if (result.RequiresTwoFactor)
            return ESignInResult.RequiresTwoFactor;
        else if (result.Succeeded)
            return ESignInResult.Succeeded;
        else
            return ESignInResult.Failed;
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var state = await authProvider.GetAuthenticationStateAsync();
        if (state is null)
        {
            return null;
        }
        var user = state.User;
        if (user is null)
        {
            return null;
        }
        var identity = user.Identity;
        if (identity is null || !identity.IsAuthenticated)
        {
            return null;
        }

        string? userEmail = identity.Name;

        using var dbContext = dbContextFactory.CreateDbContext();
        ApplicationUser? appUser = await dbContext.Users.AsNoTracking().OrderBy(r => r.Email).FirstOrDefaultAsync(r => r.Email == userEmail);

        return appUser;
    }

    public async Task<(ApplicationUser?, string)> GetCurrentUserAndEUserRoleAsync()
    {
        var state = await authProvider.GetAuthenticationStateAsync();
        if (state is null)
        {
            return (null, string.Empty); ;
        }
        var user = state.User;
        if (user is null)
        {
            return (null, string.Empty); ;
        }
        var identity = user.Identity;
        if (identity is null || !identity.IsAuthenticated)
        {
            return (null, string.Empty); ;
        }

        string? userEmail = identity.Name;

        using var dbContext = dbContextFactory.CreateDbContext();
        ApplicationUser? appUser
            = await dbContext.Users
                    .AsNoTracking()
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .OrderBy(r => r.Email)
                    .FirstOrDefaultAsync(r => r.Email == userEmail);

        var role = DetermineEUserRole(appUser);

        return (appUser, role);
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        using var dbContext = dbContextFactory.CreateDbContext();

        return await dbContext.Users.AsNoTracking().OrderBy(r => r.Email).FirstOrDefaultAsync(r => r.Email == email);
    }

    public async Task<(ApplicationUser?, string)> GetUserAndEUserRoleByEmailAsync(string email)
    {
        using var dbContext = dbContextFactory.CreateDbContext();

        ApplicationUser? user
            = await dbContext.Users
                    .AsNoTracking()
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .OrderBy(r => r.Email)
                    .FirstOrDefaultAsync(r => r.Email == email);

        var role = DetermineEUserRole(user);
        return (user, role);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(Guid userId)
    {
        using var dbContext = dbContextFactory.CreateDbContext();

        return await dbContext.Users.AsNoTracking().OrderBy(r => r.Email).FirstOrDefaultAsync(r => r.Id == userId);
    }

    public async Task<(ApplicationUser?, string)> GetUserAndEUserRoleByIdAsync(Guid id)
    {
        using var dbContext = dbContextFactory.CreateDbContext();

        ApplicationUser? user
            = await dbContext.Users
                    .AsNoTracking()
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .OrderBy(r => r.Id)
                    .FirstOrDefaultAsync(r => r.Id == id);

        var role = DetermineEUserRole(user);
        return (user, role);
    }

    public async Task<string> GetUserRoleAsync(Guid userId)
    {
        using var dbContext = dbContextFactory.CreateDbContext();

        var appUser = await dbContext.Users.AsNoTracking().Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(r => r.Id == userId);
        return DetermineEUserRole(appUser);
    }

    private static string DetermineEUserRole(ApplicationUser? appUser)
    {
        if (appUser is not null && appUser.UserRoles.Count > 0)
        {
            var role = appUser.UserRoles.FirstOrDefault();
            return role?.Role.Name ?? string.Empty;
        }
        return string.Empty;
    }

    //public Task<EUserRole> GetUserRoleByIdAsync(Guid userId)
    //{
    //    throw new NotImplementedException();
    //}


    /// ///////////////////////////////////////////////////////////////////////////////
    //
    // This code adapted from the identity template code, from Register.razor.
    //
    ///////////////////////////////////////////////////////////////////////////////////
    ///
    public async Task<Result<Guid>> RegisterUserAsync(string email, string password, string firstName, string lastName)
    {
        var user = CreateUser(email, firstName, lastName);

        await userStore.SetUserNameAsync(user, email, CancellationToken.None);
        var emailStore = GetEmailStore();
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);

        var idResult = await userManager.CreateAsync(user, password);

        if (idResult.Succeeded)
        {
            var userId = await userManager.GetUserIdAsync(user);
            var userGuid = Guid.Parse(userId);

            return Result.Success(userGuid);
        }
        else
        {
            var errors = idResult.Errors;
            var message = string.Join('\n', errors.Select(e => $"{e.Code}: {e.Description}"));
            return Result.Failure<Guid>(UserErrors.FailedUserCreation(email, message));
        }

        // We are not doing email confirmation.

        //var userId = await UserManager.GetUserIdAsync(user);
        //var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
        //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        //var callbackUrl = NavigationManager.GetUriWithQueryParameters(
        //    NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
        //    new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });

        //await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

        //if (UserManager.Options.SignIn.RequireConfirmedAccount)
        //{
        //    RedirectManager.RedirectTo(
        //        "Account/RegisterConfirmation",
        //        new() { ["email"] = Input.Email, ["returnUrl"] = ReturnUrl });
        //}

        // This remains the responsibility of the UI -- and we won't be ding it.
        //await SignInManager.SignInAsync(user, isPersistent: false);
        //RedirectManager.RedirectTo(ReturnUrl);
    }

    static ApplicationUser CreateUser(string email, string firstName, string lastName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
        };
        return user;
    }


    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)userStore;
    }

}
