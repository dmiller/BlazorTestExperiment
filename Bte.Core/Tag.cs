namespace Bte.Core;

public class Tag : EntityBase
{
    public required string Name { get; set; }

    public List<Post> Posts { get; } = [];
}
