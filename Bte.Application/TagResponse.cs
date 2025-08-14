namespace Bte.Application;

public class TagResponse
{
    public required string Name { get; set; }

    public List<PostResponse> Posts { get; } = [];
}
