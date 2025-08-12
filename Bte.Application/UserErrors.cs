using Bte.MediatR;

namespace Bte.Application;

public static class UserErrors
{
    public const string NotLoggedInErrorCode = "Users.NotLoggedIn";
    public const string UserNotFoundErrorCode = "Users.UserNotFound";
    public const string FailedAdminPasswordChangeErrorCode = "Users.FailedAdminPasswordChange";
    public const string FailedUserCreationErrorCode = "Users.FailedUserCreation";

    public static Error FailedAdminPasswordChange(Guid userId) => Error.Failure(
        FailedAdminPasswordChangeErrorCode,
        $"Failed to change password for user with Id = '{userId}'");

    public static Error FailedUserCreation(string email, string message) => Error.Failure(
        FailedUserCreationErrorCode,
        $"Failed to create user with email '{email}'.\n{message}");

    public static Error NotLoggedIn() => Error.Failure(
        NotLoggedInErrorCode,
        "User is not logged in. Please log in to perform this action.");

    public static Error UserNotFound(Guid userId) => Error.NotFound(
        UserNotFoundErrorCode,
        $"User with Id = '{userId}' was not found.");
}
