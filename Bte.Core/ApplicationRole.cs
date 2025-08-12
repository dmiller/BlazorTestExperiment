using Microsoft.AspNetCore.Identity;

namespace Bte.Core;


public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = [];

}
