namespace Bte.Core;

public class Blog : EntityBase
{
    public required string Name { get; set; }

    public IList<Post> Posts { get; } = [];
}