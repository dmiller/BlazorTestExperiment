using Bte.Application;
using Bte.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bte.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("ApplicationConnection")
                ?? throw new InvalidOperationException("Connection string 'ApplicationConnection' not found.")));
        services.AddSingleton<IApplicationDbContextFactory, ApplicationDbContextFactory>();

        AddAuthentication(services);

        return services;
    }

    private static void AddAuthentication(IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

        services.AddCascadingAuthenticationState();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
            .AddIdentityCookies();

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager<SignInManager<ApplicationUser>>()
        .AddDefaultTokenProviders();
    }

}
