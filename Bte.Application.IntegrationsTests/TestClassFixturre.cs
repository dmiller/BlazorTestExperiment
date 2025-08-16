using Bte.Infrastructure;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace Bte.Application.IntegrationsTests;

public class TestClassFixture : IAsyncLifetime
{
    private const string Database = "TestDb";
    private const string Username = "sa";
    private const string Password = "yourStrong(!)Password";
    private const ushort MsSqlPort = 1433;

    private WebApplicationFactory<Program> _factory = default!;
    private MsSqlContainer _container = default!;

    public async Task InitializeAsync()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(Password)
            .WithPortBinding(MsSqlPort, true)
            .WithEnvironment("SQLCMDUSER", Username)
            .WithEnvironment("SQLCMDPASSWORD", Password)
            .WithEnvironment("MSSQL_SA_PASSWORD", Password)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(MsSqlPort))
            .Build();

        await _container.StartAsync();

        var host = _container.Hostname;
        var port = _container.GetMappedPublicPort(1433);

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:ApplicationConnection", _container.GetConnectionString());
                //builder.ConfigureServices(services =>
                //{
                //    services.AddDbContext<IApplicationDbContext>(options =>
                //        options.UseSqlServer(connectionString));
                //    services.AddDbContextFactory<IApplicationDbContextFactory>(options =>
                //        options.UseSqlServer(connectionString));
                //});
            });

        using var scope = _factory.Services.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IApplicationDbContextFactory>();
        using var dbContext = await dbContextFactory.CreateApplicationDbContextAsync(CancellationToken.None);
        ((ApplicationDbContext)dbContext).Database.Migrate();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public WebApplicationFactory<Program> Factory => _factory;

    public IApplicationDbContextFactory GetApplicationDbContextFactory() =>
        _factory.Services.GetRequiredService<IApplicationDbContextFactory>();

    public async Task<IApplicationDbContext> GetApplicationDbContextAsync()
    {
        var dbContextFactory = GetApplicationDbContextFactory();
        return await dbContextFactory.CreateApplicationDbContextAsync(CancellationToken.None);
    }

    public IServiceScope CreateScope() => _factory.Services.CreateScope();

    public T? GetScopedService<T>(IServiceScope scope)
    {
        if (scope == null) throw new ArgumentNullException(nameof(scope));
        return scope.ServiceProvider.GetService<T>();
    }
}


