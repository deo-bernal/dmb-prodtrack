using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;

namespace ProdTrack.Application;

public static class DependencyInjection
{
    /// <summary>Registers handlers, validators, decorators (validation, authorization) and the dispatcher.</summary>
    public static IServiceCollection AddProdTrackApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped, includeInternalTypes: true);

        // Order: authorization runs first (outermost), then validation, then concurrency translation, then the handler.
        services.Decorate(typeof(ICommandHandler<,>), typeof(ConcurrencyCommandDecorator<,>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationCommandDecorator<,>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(AuthorizationCommandDecorator<,>));
        services.Decorate(typeof(IQueryHandler<,>), typeof(AuthorizationQueryDecorator<,>));

        services.AddScoped<IDispatcher, Dispatcher>();
        return services;
    }
}
