namespace Bte.Application;

/// Return status from sign-in operations.  Matches Microsoft.AspNetCore.Identity.SignInResult states.
public enum ESignInResult
{
    Succeeded,              // the sign-in was successful
    IsLockedOut,            // the user attempting to sign-in is locked out
    IsNotAllowed,           // the user attempting to sign-in is not allowed to sign-in
    RequiresTwoFactor,      // the user attempting to sign-in requires two factor authentication
    Failed                  // the sign-in failed
}
