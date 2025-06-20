using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MitMediator;

public static class DependencyInjection
{
    public static IServiceCollection AddMitMediator(this IServiceCollection services, params Assembly[] assembly)
    {
        var allTypes = assembly.SelectMany(a => a.GetTypes()).ToArray();
        
        return services
            .AddScoped<IMediator, Mediator>()
            .AddHandlers(allTypes);
    }
    
    public static IServiceCollection AddMitMediator(this IServiceCollection services)
    {
        return services.AddMitMediator(AppDomain.CurrentDomain.GetAssemblies());
    }

    private static IServiceCollection AddHandlers(this IServiceCollection services, Type[] allTypes)
    {
        var interfaceType = typeof(IHandler<,>);
        var implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddTransient(
                implementation.GetInterfaces().Single(c => c.GetGenericTypeDefinition() == typeof(IHandler<,>)),
                implementation);
        }
        
        interfaceType = typeof(Tasks.IRequestHandler<,>);
        implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddTransient(
                implementation.GetInterfaces().Single(c => c.GetGenericTypeDefinition() == typeof(Tasks.IRequestHandler<,>)),
                implementation);
        }
        
        interfaceType = typeof(IHandler<>);
        implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddTransient(
                implementation.GetInterfaces().Single(c => c.GetGenericTypeDefinition() == typeof(IHandler<>)),
                implementation);
        }

        
        interfaceType = typeof(Tasks.IRequestHandler<>);
        implementations = allTypes
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

        foreach (var implementation in implementations)
        {
            services = services.AddTransient(
                implementation.GetInterfaces().Single(c => c.GetGenericTypeDefinition() == typeof(Tasks.IRequestHandler<>)),
                implementation);
        }
        
        return services;
    }
}