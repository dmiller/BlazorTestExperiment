using Bte.Core;
using Bte.MediatR;

namespace Bte.Application;

public static class GetUserByEmail
{
    public sealed record Query(string Email) : IQuery<ApplicationUser>;

    internal sealed class Handler(IUserService userService) : IQueryHandler<Query, ApplicationUser>
    {
        public async Task<Result<ApplicationUser>> Handle(Query command, CancellationToken cancellationToken)
        {
            var user = await userService.GetUserByEmailAsync(command.Email);
            return user is not null ? Result.Success(user) : Result.Failure<ApplicationUser>(AuthenticationErrors.UserNotFound(command.Email));
        }
    }

}
