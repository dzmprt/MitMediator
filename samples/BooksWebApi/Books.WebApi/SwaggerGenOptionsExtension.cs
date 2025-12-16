using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Books.WebApi;

internal static class SwaggerGenOptionsExtension
{
    public static void ConfigureSwagger(this SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "MitMediator project Books.WebApi sample.",
            Description = "Github: https://github.com/dzmprt/MitMediator",
        });
        
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    }
}