using Bte.MediatR;

namespace Bte.Application;


public static class AuthenticationErrors
{
    public static Error UserNotFound(string email) => Error.NotFound(
        "User.NotFound",
        $"The user with the email = '{email}' was not found");

    public static Error UnconfirmedUser(string email) => Error.Failure(
        "User.Unconfirmed",
        $"The user with the email = '{email}' is not confirmed");

    public static Error UserPasswordChangeFailed(string message) => Error.Failure(
        "User.PasswordChangeFailed",
        $"Failed to change password.\n{message}");

}
