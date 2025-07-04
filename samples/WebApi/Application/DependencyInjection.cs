using System.Reflection;
using Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MitMediator;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
            .AddMitMediator()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true)
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }
}