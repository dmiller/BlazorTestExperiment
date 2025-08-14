using Bte.MediatR;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Bte.Application;

public static class GetPostsByTitleFragment
{
    public record Query(string TitleFragment) : IQuery<List<PostResponse>>;

    public class Handler(IApplicationDbContextFactory dbContextFactory) : IQueryHandler<Query, List<PostResponse>>
    {
        public async Task<Result<List<PostResponse>>> Handle(Query query, CancellationToken cancellationToken)
        {
            using var dbContext = await dbContextFactory.CreateApplicationDbContextAsync(cancellationToken);
            var posts = dbContext.Posts.AsNoTracking().Where(p => p.Title.ToUpper().Contains(query.TitleFragment, StringComparison.OrdinalIgnoreCase)).ToList();
            return Result.Success(posts.Adapt<List<PostResponse>>());
        }
    }
}
