using Bte.Core;
using Bte.MediatR;

namespace Bte.Application.IntegrationsTests;

public class PostHandlerTests : IClassFixture<TestClassFixture>
{

    private readonly TestClassFixture _fixture;
    public PostHandlerTests(TestClassFixture fixture)
    {
        _fixture = fixture;
    }
    [Fact]
    public async Task GetPostById_ShouldReturnPost_WhenPostExists()
    {
        // Arrange
        using var dbContext = await _fixture.GetApplicationDbContextAsync();

        var blog = new Blog
        {
            Name = "Test Blog",
        };

        var post = new Post
        {
            Title = "Test Post",
            Content = "This is a test post.",
            Blog = blog,
        };

        dbContext.Blogs.Add(blog);
        dbContext.Posts.Add(post);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        using var scope = _fixture.CreateScope();
        var handler = _fixture.GetScopedService<IQueryHandler<GetPostById.Query, PostResponse>>(scope);

        // Act
        var result = await handler!.Handle(new GetPostById.Query(post.Id), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(post.Title, result.Value.Title);
        Assert.Equal(post.Content, result.Value.Content);
    }

    [Fact]
    public async Task GetPostById_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        using var dbContext = await _fixture.GetApplicationDbContextAsync();
        using var scope = _fixture.CreateScope();
        var handler = _fixture.GetScopedService<IQueryHandler<GetPostById.Query, PostResponse>>(scope);

        // Act
        var result = await handler!.Handle(new GetPostById.Query(999), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Posts.NotFound", result.Error.Code);
    }
}
