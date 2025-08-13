using Bte.Application;
using Bte.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Bte.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
   : IdentityDbContext<
       ApplicationUser, ApplicationRole, Guid,
       IdentityUserClaim<Guid>, ApplicationUserRole, IdentityUserLogin<Guid>,
       IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>(options), IApplicationDbContext
{
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<ApplicationRole> ApplicationRoles { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Tag> Tags { get; set; }

    Task IApplicationDbContext.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return this.SaveChangesAsync(cancellationToken);
    }

    EntityEntry IApplicationDbContext.Entry(object entity)
    {
        return this.Entry(entity);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            // Each User can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });

        builder.Entity<ApplicationRole>(b =>
        {
            // Each Role can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();


        });

        builder.Entity<Post>()
            .HasMany(e => e.Tags)
            .WithMany(e => e.Posts)
            .UsingEntity("PostTag",
                r => r.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagsId").HasPrincipalKey(nameof(Tag.Id)),
                l => l.HasOne(typeof(Post)).WithMany().HasForeignKey("PostsId").HasPrincipalKey(nameof(Post.Id)),
                j => j.HasKey("PostsId", "TagsId"));



    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
                .UseSeeding((context, _) =>
                {
                    var getRole = context.Set<ApplicationRole>().FirstOrDefault();
                    if (getRole is null)
                    {
                        AddInitialRoles(context.Set<ApplicationRole>());
                        context.SaveChanges();
                    }
                })
                .UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    var getRole = await context.Set<ApplicationRole>().FirstOrDefaultAsync(cancellationToken);
                    if (getRole is null)
                    {
                        AddInitialRoles(context.Set<ApplicationRole>());
                        await context.SaveChangesAsync(cancellationToken);
                    }
                });
    }

    private static void AddInitialRoles(DbSet<ApplicationRole> roles)
    {
        roles.Add(new ApplicationRole
        {
            Name = "Superuser",
            NormalizedName = "SUPERUSER",
            Description = "IT administrator role with full access",
        });
        roles.Add(new ApplicationRole
        {
            Name = "Admin",
            NormalizedName = "ADMIN",
            Description = "Administrator with full access",
        });
        roles.Add(new ApplicationRole
        {
            Name = "User",
            NormalizedName = "USER",
            Description = "Regular user",
        });
    }

}
