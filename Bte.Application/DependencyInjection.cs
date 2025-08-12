using Bte.MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Bte.Application;

public static class DependencyInjection
{

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(typeof(DependencyInjection).Assembly);

        return services;
    }

}
