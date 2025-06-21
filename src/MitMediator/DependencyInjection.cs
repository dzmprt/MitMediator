using System.Reflection;
using MitMediator;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddMitMediator(this IServiceCollection services, params Assembly[] assembly)
    {
        var allTypes = assembly.SelectMany(a => a.GetTypes()).ToArray();

        return services
            .AddScoped<IMediator, Mediator>()
            .AddNotificationHandlers(allTypes)
            .AddRequestHandlers(allTypes)
            .AddStreamRequestHandlers(allTypes);
    }

    public static IServiceCollection AddMitMediator(this IServiceCollection services)
    {
        return services.AddMitMediator(AppDomain.CurrentDomain.GetAssemblies());
    }

    private static IServiceCollection AddNotificationHandlers(this IServiceCollection services, Type[] allTypes)
    {
        var interfaceType = typeof(INotificationHandler<>);
        var implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddTransient(
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
            services = services.AddTransient(
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
            services = services.AddTransient(
                implementation.GetInterfaces().Single(c =>
                    c.IsGenericType && c.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)),
                implementation);
        }

        interfaceType = typeof(MitMediator.Tasks.IRequestHandler<,>);
        implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddTransient(
                implementation.GetInterfaces().Single(c =>
                    c.IsGenericType && c.GetGenericTypeDefinition() == typeof(MitMediator.Tasks.IRequestHandler<,>)),
                implementation);
        }

        interfaceType = typeof(IRequestHandler<>);
        implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddTransient(
                implementation.GetInterfaces().Single(c =>
                    c.IsGenericType && c.GetGenericTypeDefinition() == typeof(IRequestHandler<>)),
                implementation);
        }


        interfaceType = typeof(MitMediator.Tasks.IRequestHandler<>);
        implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddTransient(
                implementation.GetInterfaces().Single(c =>
                    c.IsGenericType && c.GetGenericTypeDefinition() == typeof(MitMediator.Tasks.IRequestHandler<>)),
                implementation);
        }

        return services;
    }
}