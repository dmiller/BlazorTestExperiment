namespace Bte.Application;

public class PostResponse
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedOn { get; set; }

    public required BlogResponse Blog { get; set; }

    public List<TagResponse> Tags { get; } = [];
}
