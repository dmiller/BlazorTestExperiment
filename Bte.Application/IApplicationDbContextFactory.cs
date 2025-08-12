namespace Bte.Application;

public interface IApplicationDbContextFactory
{
    Task<IApplicationDbContext> CreateApplicationDbContextAsync(CancellationToken cancellationToken);
}