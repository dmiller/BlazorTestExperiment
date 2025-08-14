using Bte.MediatR;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Bte.Application;

public static class GetPostById
{
    public record Query(int Id) : IQuery<PostResponse>;

    public class Handler(IApplicationDbContextFactory dbContextFactory) : IQueryHandler<Query, PostResponse>
    {

        public async Task<Result<PostResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            using var dbContext = await dbContextFactory.CreateApplicationDbContextAsync(cancellationToken);

            var post = await dbContext.Posts.AsNoTracking().OrderBy(p => p.Id).FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (post == null)
            {
                return Result.Failure<PostResponse>(Error.NotFound("Posts.NotFound", $"Post with id {request.Id} not found."));
            }
            return Result.Success(post.Adapt<PostResponse>());
        }
    }
}
