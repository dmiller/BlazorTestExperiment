namespace Bte.Application;

public class BlogResponse
{
    public string Name { get; set; } = string.Empty;

    public List<PostResponse> Posts { get; } = [];
}
