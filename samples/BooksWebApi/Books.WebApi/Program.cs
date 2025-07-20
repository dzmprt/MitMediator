using System.Reflection;
using Books.Application;
using Books.Infrastructure;
using Books.WebApi;
using Microsoft.OpenApi.Models;
using Books.WebApi.Endpoints;
using Books.WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Books api", Version = "v1", Description = "Sample API project", Contact = new OpenApiContact()
    {
        Name = "MitMediator project",
        Url = new Uri("https://github.com/dzmprt/MitMediator")
    } });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices();

var app = builder.Build();

app.InitDatabase();

app.UseCustomExceptionsHandler();

app.UseAuthorsApi();
app.UseGenresApi();
app.UseBooksApi();

app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentname}/swagger.json"; })
    .UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "swagger";
    });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
