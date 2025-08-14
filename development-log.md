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
    - `Mapster`

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

## Something to test

My first goal is to some integration tests for commands/handlers.  I'm not ready to try to do that the two commands related to user login/registration.
And not sure it is really necessary.  They are straight calls to the identity services.  Better to test that stuff in acceptance tests at the UI level.

So I"m going to create a few entities and some queries and commands manipulating them.
Let's go with the familiar.  I stole directly from the `Blog` + `Post` + `Tag` + `PostTag` example from the EF Core [relationships documentation](https://learn.microsoft.com/en-us/ef/core/modeling/relationships).

- Add those classes (except `PostTag` -- we'll let that be determined).
- Add `DbSet` properties to `IApplicationDbContext` and `ApplicationDbContext`.
- Create a migration and update the database.

The first testing goal is to do integration tests for the queries and commands related to these entities.
So we need queries and commands.
Just a few will do.

## Looking at Testcontainers

I found a few good tutorials explaining how to set up Testcontainers.Net. Most of these deal with SqlServer as a primary focus.
Several address migrations.

- [How to Use TestContainers in .Net](https://www.freecodecamp.org/news/how-to-use-testcontainers-in-net/)
    - Especially good on `IAsyncLifetime` with `IClassFixture` and `ICollectionFixture`.
- [Integration Testing using Testcontainers in .NET 8](https://medium.com/codenx/integration-testing-using-testcontainers-in-net-8-520e8911d081)
    - Nice writeup.
    - Has one line of code for migration.
- [How to use Testcontainers with .NET Unit Tests](https://blog.jetbrains.com/dotnet/2023/10/24/how-to-use-testcontainers-with-dotnet-unit-tests/) 
    - His [github repo](https://github.com/khalidabuhakmeh/TestingWithContainers)
    - Decent
- [Testcontainers Best Practices for .NET Integration Testing, Milan Jovanović](https://www.milanjovanovic.tech/blog/testcontainers-best-practices-dotnet-integration-testing)
- [The Best Way To Use Docker For Integration Testing In .NET, Milan Jovanović, 19 minutes](https://www.youtube.com/watch?v=tj5ZCtvgXKY)
    - Okay, but a little too specific to minimal API testing.

- [Coding Shorts 108: Using TestContainers in .NET Core, Shawn Wildermuth, 17 minutes](https://www.youtube.com/watch?v=vy1aIT5Ppj8)

- [Testing Entity Framework Core Correctly in .NET, Nick Chapsas, 8 minutes](https://www.youtube.com/watch?v=m7r2qyUabTs&t=2s)
    - Marginal, touches on TestContainers on the end.  Does suggest how to override the connection string for a webhosting app.

- [The cleanest way to use Docker for testing in .NET, Nick Chapsas, 14 minutes](https://www.youtube.com/watch?v=8IRNC7qZBmk)
    - Not as in-depth as some of the others.


Worth noting:

- [Blazor-testing from A to Z - Egil Hansen - NDC London 2025. 58 minutes](https://www.youtube.com/watch?v=p-H5fEMCB8s)

Documentation: Microsoft

- [Testing against your production database system](https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database)

Documentation: Testcontainers.net

- [Testcontainers.Net](https://testcontainers.com/)
- [Getting started with Testcontainers for .NET](https://testcontainers.com/guides/getting-started-with-testcontainers-for-dotnet/)
- [Testing an ASP.NET Core web app](https://testcontainers.com/guides/testing-an-aspnet-core-web-app/)
- [Testing with xUnit.net](https://dotnet.testcontainers.org/test_frameworks/xunit_net/)
- [ASP.NET Core (example)](https://dotnet.testcontainers.org/examples/aspnet/)
- [Testcontainers for .Net](https://dotnet.testcontainers.org/)
- [MSSQL (module)](https://testcontainers.com/modules/mssql/)
- [Xunit (module)](https://testcontainers.com/modules/xunit/?language=dotnet)

## An integration test project

- Create an xUnit test project in the `test\` solution folder.
- Add packages:
  - `Testcontainers`
  - `Testcontainers.MsSql`
  - `FluentAssertions`
- Delete `UnitTest1.cs`.



