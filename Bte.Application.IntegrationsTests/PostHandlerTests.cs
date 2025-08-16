using Bte.Core;
using Bte.MediatR;

namespace Bte.Application.IntegrationsTests;

public class PostHandlerTests(IntegrationTestWebAppFactory factory) : IClassFixture<IntegrationTestWebAppFactory>
{
    [Fact]
    public async Task GetPostById_ShouldReturnPost_WhenPostExists()
    {
        // Arrange
        using var dbContext = await factory.GetApplicationDbContextAsync();

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

        using var scope = factory.CreateScope();
        var handler = IntegrationTestWebAppFactory.GetScopedService<IQueryHandler<GetPostById.Query, PostResponse>>(scope);

        // Act
        var result = await handler!.Handle(new GetPostById.Query(post.Id), CancellationToken.None);

        dbContext.ChangeTracker.Clear(); // Clear the change tracker to avoid tracking issues

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
        using var dbContext = await factory.GetApplicationDbContextAsync();
        using var scope = factory.CreateScope();
        var handler = IntegrationTestWebAppFactory.GetScopedService<IQueryHandler<GetPostById.Query, PostResponse>>(scope);

        // Act
        var result = await handler!.Handle(new GetPostById.Query(999), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Posts.NotFound", result.Error.Code);
    }
}
