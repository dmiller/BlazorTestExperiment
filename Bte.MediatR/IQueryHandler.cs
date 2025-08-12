namespace Bte.MediatR;


public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}


public interface IQueryHandler<in TQuery, TResponse, TError>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse, TError>> Handle(TQuery query, CancellationToken cancellationToken);
}
