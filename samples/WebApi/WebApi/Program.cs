using System.Reflection;
using Application;
using Application.UseCase.Authors.Commands.CreateAuthor;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MitMediator;
using WebApi.Endpoints;
using WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Books api", Version = "v1", Description = "Sample API project" });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices();

var app = builder.Build();

InitDatabase(app);

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

void InitDatabase(WebApplication webApplication)
{
    using var scope = webApplication.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    var authors = new List<Author>
    {
        new Author("Clara", "Jennings"),
        new Author("Thomas", "McRae"),
        new Author("Elise", "Duvall"),
        new Author("Devin", "Arkwell"),
        new Author("Jun", "Nakamura"),
        new Author("Mia", "Talbot"),
        new Author("Isabelle", "North"),
        new Author("Marcus", "Ellwood"),
        new Author("Ava", "Klein"),
        new Author("Rowan", "Thorne"),
        new Author("Keira", "Sunfall"),
        new Author("Eliot", "Greaves"),
        new Author("Blake", "Monty"),
        new Author("Felicity", "Crumb"),
        new Author("Grant", "Peebles"),
    };

    db.Authors.AddRange(authors);

    var genres = new List<Genre>
    {
        new Genre("Mystery / Thriller"),
        new Genre("Science Fiction"),
        new Genre("Psychological Drama"),
        new Genre("Fantasy"),
        new Genre("Comedy / Satire"),
    };

    db.Genres.AddRange(genres);
    db.SaveChanges();

    var books = new List<Book>()
    {
        new Book("The Vanishing Hour", authors[0], genres[0]),
        new Book("Ashes Beneath", authors[0], genres[0]),
        new Book("Dead Man’s Loop", authors[1], genres[0]),
        new Book("Code Silence", authors[1], genres[0]),
        new Book("The Crows Know", authors[2], genres[0]),
        new Book("Glass Evidence", authors[2], genres[0]),
        new Book("Ion Halo", authors[3], genres[1]),
        new Book("Echo Code", authors[3], genres[1]),
        new Book("Between the Stars", authors[4], genres[1]),
        new Book("Signal Divide", authors[4], genres[1]),
        new Book("Genesis Protocol", authors[5], genres[1]),
        new Book("The Fifth Mind", authors[5], genres[1]),
        new Book("Things We Leave Behind", authors[6], genres[2]),
        new Book("Reflections of Her", authors[6], genres[2]),
        new Book("The Weight of Rain", authors[7], genres[2]),
        new Book("Quiet Hours", authors[7], genres[2]),
        new Book("Unfinished Letters", authors[8], genres[2]),
        new Book("A Thread Through Glass", authors[8], genres[2]),
        new Book("The Ember Throne", authors[9], genres[3]),
        new Book("Moonblade Pact", authors[9], genres[3]),
        new Book("Whispers of the Hollow", authors[10], genres[3]),
        new Book("Stormforged Oath", authors[10], genres[3]),
        new Book("The Last Oracle", authors[11], genres[3]),
        new Book("Chronicles of Eldenmere", authors[11], genres[3]),
        new Book("How to Raise a Platypus", authors[12], genres[4]),
        new Book("Management for Goblins", authors[12], genres[4]),
        new Book("Midlife Crisps", authors[13], genres[4]),
        new Book("Interviews with My Cat", authors[13], genres[4]),
        new Book("The Bureau of Odd Affairs", authors[14], genres[4]),
        new Book("Toothpaste Politics", authors[14], genres[4]),
    };
    db.Books.AddRange(books);
    db.SaveChanges();
}