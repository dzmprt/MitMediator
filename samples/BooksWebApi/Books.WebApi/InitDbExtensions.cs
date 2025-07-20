using Books.Domain;
using Books.Infrastructure;

namespace Books.WebApi;

public static class InitDbExtensions
{
    public static void InitDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();

        var authors = new List<Author>
        {
            new("Clara", "Jennings"),
            new("Thomas", "McRae"),
            new("Elise", "Duvall"),
            new("Devin", "Arkwell"),
            new("Jun", "Nakamura"),
            new("Mia", "Talbot"),
            new("Isabelle", "North"),
            new("Marcus", "Ellwood"),
            new("Ava", "Klein"),
            new("Rowan", "Thorne"),
            new("Keira", "Sunfall"),
            new("Eliot", "Greaves"),
            new("Blake", "Monty"),
            new("Felicity", "Crumb"),
            new("Grant", "Peebles"),
        };

        db.Authors.AddRange(authors);

        var genres = new List<Genre>
        {
            new("Mystery / Thriller"),
            new("Science Fiction"),
            new("Psychological Drama"),
            new("Fantasy"),
            new("Comedy / Satire"),
        };

        db.Genres.AddRange(genres);
        db.SaveChanges();

        var books = new List<Book>()
        {
            new("The Vanishing Hour", authors[0], genres[0]),
            new("Ashes Beneath", authors[0], genres[0]),
            new("Dead Man’s Loop", authors[1], genres[0]),
            new("Code Silence", authors[1], genres[0]),
            new("The Crows Know", authors[2], genres[0]),
            new("Glass Evidence", authors[2], genres[0]),
            new("Ion Halo", authors[3], genres[1]),
            new("Echo Code", authors[3], genres[1]),
            new("Between the Stars", authors[4], genres[1]),
            new("Signal Divide", authors[4], genres[1]),
            new("Genesis Protocol", authors[5], genres[1]),
            new("The Fifth Mind", authors[5], genres[1]),
            new("Things We Leave Behind", authors[6], genres[2]),
            new("Reflections of Her", authors[6], genres[2]),
            new("The Weight of Rain", authors[7], genres[2]),
            new("Quiet Hours", authors[7], genres[2]),
            new("Unfinished Letters", authors[8], genres[2]),
            new("A Thread Through Glass", authors[8], genres[2]),
            new("The Ember Throne", authors[9], genres[3]),
            new("Moonblade Pact", authors[9], genres[3]),
            new("Whispers of the Hollow", authors[10], genres[3]),
            new("Stormforged Oath", authors[10], genres[3]),
            new("The Last Oracle", authors[11], genres[3]),
            new("Chronicles of Eldenmere", authors[11], genres[3]),
            new("How to Raise a Platypus", authors[12], genres[4]),
            new("Management for Goblins", authors[12], genres[4]),
            new("Midlife Crisps", authors[13], genres[4]),
            new("Interviews with My Cat", authors[13], genres[4]),
            new("The Bureau of Odd Affairs", authors[14], genres[4]),
            new("Toothpaste Politics", authors[14], genres[4]),
        };
        db.Books.AddRange(books);
        db.SaveChanges();
    }
}