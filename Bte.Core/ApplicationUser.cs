using Microsoft.AspNetCore.Identity;

namespace Bte.Core;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
}


