using Bte.Application;
using Microsoft.EntityFrameworkCore;

namespace Bte.Infrastructure;

public class ApplicationDbContextFactory(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IApplicationDbContextFactory
{
    public async Task<IApplicationDbContext> CreateApplicationDbContextAsync(CancellationToken cancellationToken)
    {
        return await dbContextFactory.CreateDbContextAsync(cancellationToken);
    }
}
