using Bte.Core;
using Bte.MediatR;

namespace Bte.Application;

public interface IUserService
{
    Task<ApplicationUser?> GetCurrentUserAsync();
    Task<ApplicationUser?> GetUserByIdAsync(Guid userId);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);

    Task<string> GetUserRoleAsync(Guid userId);

    Task<(ApplicationUser?, string)> GetCurrentUserAndEUserRoleAsync();
    Task<(ApplicationUser?, string)> GetUserAndEUserRoleByIdAsync(Guid userId);
    Task<(ApplicationUser?, string)> GetUserAndEUserRoleByEmailAsync(string email);

    Task<ESignInResult> LoginUserAsync(string username, string password, bool rememberMe);
    Task<Result<Guid>> RegisterUserAsync(string email, string password, string firstName, string lastName);


}
