namespace Bte.Core;

public class Post : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedOn { get; set; }

    public required Blog Blog { get; set; }

    public List<Tag> Tags { get; } = [];
}

