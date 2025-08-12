using Bte.MediatR;

namespace Bte.Application;

public static class LoginUser
{
    public sealed record Command(string Username, string Password, bool RememberMe) : ICommand<ESignInResult>;

    // This call always succeeds.  The ESignInResult gives more detail.

    internal sealed class Handler(IUserService authenticationService) : ICommandHandler<Command, ESignInResult>
    {
        public async Task<Result<ESignInResult>> Handle(Command command, CancellationToken cancellationToken) =>
            await authenticationService.LoginUserAsync(command.Username, command.Password, command.RememberMe);
    }
}
