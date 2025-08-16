using Bte.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bte.Application;

public interface IApplicationDbContext : IDisposable
{
    // DbSets for all entity classes we need access to

    DbSet<ApplicationUser> ApplicationUsers { get; set; }
    DbSet<ApplicationRole> ApplicationRoles { get; set; }
    DbSet<Blog> Blogs { get; set; }
    DbSet<Post> Posts { get; set; }
    DbSet<Tag> Tags { get; set; }


    // DbContext methods we need direct access to
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    EntityEntry Entry(object entity);
    EntityEntry Remove(object entity);
    EntityEntry Attach(object entity);
    DatabaseFacade Database { get; }

}
