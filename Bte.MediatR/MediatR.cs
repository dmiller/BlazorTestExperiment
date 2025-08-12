using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bte.MediatR;

public static class Mediator
{
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = [Assembly.GetExecutingAssembly()];
        }

        services.Scan(scan => scan.FromAssemblies(assemblies)
          .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
              .AsImplementedInterfaces()
              .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,,>)), publicOnly: false)
              .AsImplementedInterfaces()
              .WithScopedLifetime()
          .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
              .AsImplementedInterfaces()
              .WithScopedLifetime()
          .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
              .AsImplementedInterfaces()
              .WithScopedLifetime()
          .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,,>)), publicOnly: false)
              .AsImplementedInterfaces()
              .WithScopedLifetime());

        return services;
    }

}

