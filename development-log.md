# BlazorTestExperiment Development Log

I'm attempting to figure out the best way to do containerizd integration tests against SQL Server for a Blazor app.  The Blazor app itself will be a minimized version of my standard Blazor app architecture.  I plan to use Testcontainers.net to manage the containers.

## Initial project setup

This is more elaborate than really necessary for this very simple project, but I want to mimic the structure of my larger projects.

- Create a blank solution.
- Create solution folders `src\` and `test\`.
- Create a new Blazor WebAssembly project in the `src\` folder.
- Create a new xUnit test project in the `test\` folder.
- Create class library projects in the `src\`:
     - `Bte.Core` -- this will contain a few entity classes
     - `Bte.Application` -- a small bit of application logic
     - `Bte.Infrastructure` -- this will contain our `DbContext` and migrations
- Get rid of the `Class1.cs1` file in each project.
- Create a Blazor Server project named `Bte.UI.Server`.
    - Authentication Type = None
        - I need to test creating users, roles, logging in during testing.
        - However, this will put an `ApplicationUser` and an initial migration in a place I don't want.
        - I'll add identity by hand later.
    - Interactive render mode
    - Interactivity location: Per page/component
    - Sample pages: Yes -- might as well have something to look at.
- Set `Bte.UI.Server` as the startup project.
- Set up package references.
    - `Bte.Application` references `Bte.Core`
    - `Bte.Infrastructure` references `Bte.Application`
    - `Bte.UI.Server` references `Bte.Infrastructure`
- Run it to make sure we are good to go.

## More setup

- Add `DependencyInjection.cs` to `Bte.Infrastructure`.
- Add stub:

```C#
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
```

- Add `DependencyInjection.cs` to `Bte.Application`.
- Add stub:

```C#
 public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
 {
    return services;
 }
```

We'll fill in those stubs shortly.

- Add calls to `AddApplication` and `AddInfrastructure`  to `Program`.

- Add our MediatR replacement project. We don't really need it, but might as well include it for completeness.

- Add reference to  `Bte.MediatR` to `Bte.Application`.
- Add service registration for `MediatR` to `Bte.Application.DependencyInjection.AddApplication`.

- Add the following to the `Logan.Infrastructure`. (I'm not sure this is minimal, but it works.)
    - `Microsoft.AspNetCore.Authentication`
    - `Microsoft.AspNetCore.Components.Authorization`
    - `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore`
    - `Microsoft.AspNetCore.Identity`
    - `Microsoft.EntityFrameworkCore`
    - `Microsoft.EntityFrameworkCore.SqlServer`

- Add to `Logan.Application`:
    - `Microsoft.Extensions.Identity.Core`

- Add to `Logan.Core`:
    - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`

And it still compiles and runs.

## User identity/authentication/authorization + initial database work

- Add to `Bte.Core`:
    - `ApplicationUser`
    - `ApplicationRole`
    - `ApplicationUserRole`

- Add to `Bte.Application`:
    - `IApplicationDbContext`
    - `IApplicationDbContextFactory`

These really are overkill for this simple project, but why not?

- Add to `Bte.Infrastructure`:
    - `ApplicationDbContext`
    - `ApplicationDbContextFactory`
- Add code for `ApplicationDbContext.OnModelCreating` to model the `ApplicationUser`, `ApplicationRole`, and `ApplicationUserRole` relationships. 
- Add code for `ApplicationDbContext.OnConfiguring` to seed initial roles.

- Add code to `Bte.Infrastructure.DependencyInjection` to register the context factory and also `IApplicationDbContext`.
- Add code also for authentication registration.

Once again just borrowing more code than we need just to avoid thinking.

- In `Bte.Application`, add
    - `IUserService`
    - `AuthenticationErrors`
    - `UserErrors`
    - `ESignInResult`
- In `Bte.Infrastructure`, add
    - `UserService`.
- In `Bte.Infrastructure.DependencyInjection`, add
    - `AddScoped<IUserService, UserService>()`.

- There are some other scoped services that live only in `Bte.UI.Server`.  These are mostly copied from the Asp.Net Core identity code.  We need to add the files and get the classes registered as services.  These are in `Logan.UI.Server\Shared\`:
    - `Cookies.cs`
    - `IdentityNoOpEmailSender.cs`
    - `IdentityRedirectManager.cs`
    - `IdentityUserAccessor.cs`    
- We also need to add the user seeding code (for development only) to `Program.cs`. 
    - Add `DevelopmentInitialization` class to `Logan.UI.Server`.
    - Call it conditionally (on environment being development) in `Program.cs`.
- Almost forgot:  Add the connection strings section to `appsettings.Development.json`.  Don't forget to change the database name.

- Time to create our database.

    - Set `Bte.UI.Server` as the startup project.
    - Set `Bte.Infrastructure` as the default project in the console.
    - And forgot to add `Microsoft.EntityFrameworkCore.Tools` package to `Bte.UI.Server`.  Doing that now.
    - `Add-Migration InitCreate -Context ApplicationDbContext -Args '--environment Development'`
    - `Update-Database -Context ApplicationDbContext -Args '--environment Development'`

    And that works.  There is an error about doing a `FirstOrDefaultAsync` with no order by.  We'll fix that later.

    _AAAANNNDDD_ we have a running application (again).

    ## Login pages, etc.

We have a database.  Now let's try to hook up login pages.  And lock down the home page so it pushes us to the login page if we are not logged in.

I'm going with a very minimal set of very much unstyled pages to get this working.

- Add the minimal set of pages to `Bte.UI.Server\Components\User`:
    - `Login.razor`
    - `Logout.razor`
    - `Lockout.razor`
    - `AccessDenied.razor`
    - `RedirectToLogin.razor`
    - `LoginLayout.razor`
    - `LoginUserModel.cs`
- Add the `LoginUser` command+handler to `Bte.Application`.

Now we need to lock out pages and force redirect to login when not authenticated.


- In `Bte.UI.Server/Compoentns/_Imports.razor` , set `@attribute [Authorize]` for all user components.
- Modify `App.razor` to pass along the page render mode to the `HeadOutlet` and to `Routes`.
- Modify `Routes.razor` to use `AuthorizeRouteView` instead of `RouteView`.  
- Modify `MainLayout.razor` to remove `About` link and add markup and code to show username, email, and link to log out.

