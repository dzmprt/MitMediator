using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MitMediator;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

public static class DependencyInjection
{
    /// <summary>
    /// Inject IMediator, IRequestHandlers, IStreamRequestHandlers and INotificationHandlers
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/>.</param>
    /// <param name="assembly"><see cref="Assembly"/>.</param>
    /// <returns><see cref="IServiceCollection"/>.</returns>
    [RequiresDynamicCode("This function is incompatible with Native AOT. Use AddOnlyMitMediator and register handlers manually.")]
    public static IServiceCollection AddMitMediator(this IServiceCollection services, params Assembly[] assembly)
    {
        var allTypes = assembly.SelectMany(a => a.GetTypes().Where(t => !t.IsAbstract)).ToArray();

        return services
            .AddScoped<IMediator, Mediator>()
            .AddNotificationHandlers(allTypes)
            .AddRequestHandlers(allTypes)
            .AddStreamRequestHandlers(allTypes);
    }


    /// <summary>
    /// Inject IMediator, IRequestHandlers, IStreamRequestHandlers and INotificationHandlers from
    /// AppDomain.CurrentDomain.GetAssemblies()
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/>.</param>
    /// <returns><see cref="IServiceCollection"/>.</returns>
    [RequiresDynamicCode("This function is incompatible with Native AOT. Use AddOnlyMitMediator and register handlers manually.")]
    public static IServiceCollection AddMitMediator(this IServiceCollection services)
    {
        return services.AddMitMediator(AppDomain.CurrentDomain.GetAssemblies());
    }

    /// <summary>
    /// Inject IMediator.
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddOnlyMitMediator(this IServiceCollection services)
    {
        return services.AddScoped<IMediator, Mediator>();
    }

    private static IServiceCollection AddNotificationHandlers(this IServiceCollection services, Type[] allTypes)
    {
        var interfaceType = typeof(INotificationHandler<>);
        var implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddScoped(
                implementation.GetInterfaces().Single(c =>
                    c.IsGenericType && c.GetGenericTypeDefinition() == typeof(INotificationHandler<>)),
                implementation);
        }

        return services;
    }

    private static IServiceCollection AddStreamRequestHandlers(this IServiceCollection services, Type[] allTypes)
    {
        var interfaceType = typeof(IStreamRequestHandler<,>);
        var implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddScoped(
                implementation.GetInterfaces().Single(c =>
                    c.IsGenericType && c.GetGenericTypeDefinition() == typeof(IStreamRequestHandler<,>)),
                implementation);
        }

        return services;
    }

    private static IServiceCollection AddRequestHandlers(this IServiceCollection services, Type[] allTypes)
    {
        var interfaceType = typeof(IRequestHandler<,>);
        var implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddScoped(
                implementation.GetInterfaces().Single(c =>
                    c.IsGenericType && c.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)),
                implementation);
        }

        interfaceType = typeof(MitMediator.Tasks.IRequestHandler<,>);
        implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddScoped(
                implementation.GetInterfaces().Single(c =>
                    c.IsGenericType && c.GetGenericTypeDefinition() == typeof(MitMediator.Tasks.IRequestHandler<,>)),
                implementation);
        }

        interfaceType = typeof(IRequestHandler<>);
        implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddScoped(
                implementation.GetInterfaces().Single(c =>
                    c.IsGenericType && c.GetGenericTypeDefinition() == typeof(IRequestHandler<>)),
                implementation);
        }


        interfaceType = typeof(MitMediator.Tasks.IRequestHandler<>);
        implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddScoped(
                implementation.GetInterfaces().Single(c =>
                    c.IsGenericType && c.GetGenericTypeDefinition() == typeof(MitMediator.Tasks.IRequestHandler<>)),
                implementation);
        }

        return services;
    }
}
