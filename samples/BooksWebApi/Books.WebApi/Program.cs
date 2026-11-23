using Books.Application;
using Books.Infrastructure;
using Books.WebApi;
using Books.WebApi.Endpoints;
using Books.WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddSwaggerGen(options => options.ConfigureSwagger())
    .AddEndpointsApiExplorer()
    .AddProblemDetails()
    .AddExceptionHandler<BooksApiExceptionHandler>()
    .AddApplicationServices()
    .AddPersistenceServices();

var app = builder.Build();

app.InitDatabase();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseAuthorsApi()
    .UseGenresApi()
    .UseBooksApi();

app.UseSwagger()
    .UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();

/// <summary>
/// Application entry point exposed for integration testing.
/// </summary>
public partial class Program;
