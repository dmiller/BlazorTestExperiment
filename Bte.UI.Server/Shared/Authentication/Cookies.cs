namespace Bte.UI.Server.Shared.Authentication;

public static class Cookies
{

    public static string? GetCookie(HttpContext? context, string cookieName)
    {
        if (context == null)
            return null;

        if (context.Request.Cookies.TryGetValue(cookieName, out var cookieValue))
            return cookieValue;
        else
            return null;
    }

    public static void SetCookie(HttpContext? context, string cookieName, string cookieValue)
    {
        if (context == null)
            return;

        context.Response.Cookies.Append(cookieValue, cookieName, new CookieOptions
        {
            HttpOnly = false,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(1) // Set expiration as needed
        });
    }

    public static void DeleteCookie(HttpContext? context, string cookieName)
    {
        if (context == null)
            return;
        context.Response.Cookies.Delete(cookieName, new CookieOptions
        {
            HttpOnly = false,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax
        });
    }
}
