using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Books.Domain.Events;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MitMediator;

namespace Books.WebApi.Tests;

public sealed class BooksApiTests : IClassFixture<BooksApiFactory>
{
    private readonly BooksApiFactory _factory;

    public BooksApiTests(BooksApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuthorLifecycle_ReturnsModernStatusesAndPublishesCommittedEvents()
    {
        using var client = _factory.CreateClient();
        _factory.Events.Clear();

        var createResponse = await client.PostAsJsonAsync("/api/v1/authors", new
        {
            firstName = "Dorothy",
            lastName = "Vaughan"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var location = createResponse.Headers.Location;
        Assert.NotNull(location);
        var author = await createResponse.Content.ReadFromJsonAsync<AuthorResponse>();
        Assert.NotNull(author);
        Assert.Equal("DOROTHY", author.FirstName);

        var createdEvent = Assert.IsType<AuthorCreatedDomainEvent>(
            Assert.Single(_factory.Events.OfType<AuthorCreatedDomainEvent>()));
        Assert.True(createdEvent.Author.AuthorId > 0);
        Assert.Empty(createdEvent.Author.DomainEvents);

        var updateResponse = await client.PutAsJsonAsync(location, new
        {
            firstName = "Dorothy Johnson",
            lastName = "Vaughan"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Single(_factory.Events.OfType<AuthorUpdatedDomainEvent>());

        var deleteResponse = await client.DeleteAsync(location);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Single(_factory.Events.OfType<AuthorDeletedDomainEvent>());
    }

    [Fact]
    public async Task MissingAuthor_ReturnsProblemDetails()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/authors/2147483647");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        Assert.Equal(404, problem?.Status);
        Assert.Equal("Not found", problem?.Title);
    }

    [Fact]
    public async Task AuthorQueries_ReturnSeededData()
    {
        using var client = _factory.CreateClient();

        var listResponse = await client.GetAsync("/api/v1/authors?limit=2&offset=1&freeText=a");
        var itemResponse = await client.GetAsync("/api/v1/authors/1");

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, itemResponse.StatusCode);
        Assert.Equal(2, (await listResponse.Content.ReadFromJsonAsync<AuthorResponse[]>())!.Length);
    }

    [Fact]
    public async Task InvalidAuthor_ReturnsValidationProblemDetails()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/authors", new
        {
            firstName = "",
            lastName = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains(
            "FirstName",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task BookLifecycle_CoversCreateUpdateDeleteAndQueries()
    {
        using var client = _factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/v1/books", new
        {
            title = "Test Driven",
            authorId = 1,
            genreName = "MYSTERY / THRILLER"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var book = await createResponse.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);

        var getResponse = await client.GetAsync($"/api/v1/books/{book.BookId}");
        var listResponse = await client.GetAsync("/api/v1/books?limit=5&offset=0&freeText=test");
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/books/{book.BookId}", new
        {
            title = "Updated",
            authorId = 2,
            genreName = "SCIENCE FICTION"
        });
        var deleteResponse = await client.DeleteAsync($"/api/v1/books/{book.BookId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task BookErrors_ReturnProblemDetails()
    {
        using var client = _factory.CreateClient();

        var missingAuthor = await client.PostAsJsonAsync("/api/v1/books", new
        {
            title = "Missing author",
            authorId = 999999,
            genreName = "FANTASY"
        });
        var missingGenre = await client.PostAsJsonAsync("/api/v1/books", new
        {
            title = "Missing genre",
            authorId = 1,
            genreName = "DOES NOT EXIST"
        });
        var missingBook = await client.GetAsync("/api/v1/books/2147483647");

        Assert.Equal(HttpStatusCode.BadRequest, missingAuthor.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, missingGenre.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingBook.StatusCode);
    }

    [Fact]
    public async Task GenreLifecycle_CoversCreateListDelete()
    {
        using var client = _factory.CreateClient();
        var genreName = $"TEST-{Guid.NewGuid():N}";
        var createResponse = await client.PostAsJsonAsync("/api/v1/genres", new
        {
            genreName
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await client.GetAsync("/api/v1/genres");
        var deleteResponse = await client.DeleteAsync($"/api/v1/genres/{genreName.ToUpperInvariant()}");

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var genres = await listResponse.Content.ReadFromJsonAsync<GenreResponse[]>();
        Assert.NotNull(genres);
        Assert.Contains(
            genres,
            genre => genre.GenreName == genreName.ToUpperInvariant());
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task GenreErrors_ReturnProblemDetails()
    {
        using var client = _factory.CreateClient();

        var duplicate = await client.PostAsJsonAsync("/api/v1/genres", new
        {
            genreName = "FANTASY"
        });
        var missing = await client.DeleteAsync("/api/v1/genres/DOES-NOT-EXIST");

        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task SwaggerAndUnknownRoute_AreAvailable()
    {
        using var client = _factory.CreateClient();

        var swagger = await client.GetAsync("/swagger/v1/swagger.json");
        var unknown = await client.GetAsync("/api/v1/unknown");

        Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);
        Assert.Contains("\"openapi\"", await swagger.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    private sealed record AuthorResponse(int AuthorId, string FirstName, string LastName);

    private sealed record ProblemDetailsResponse(int Status, string Title);

    private sealed record BookResponse(int BookId, string Title);

    private sealed record GenreResponse(string GenreName);
}

public sealed class BooksApiFactory : WebApplicationFactory<Program>
{
    public ConcurrentQueue<INotification> Events { get; } = [];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(this);
        });
    }
}

public sealed class AuthorEventRecorder(BooksApiFactory factory) :
    INotificationHandler<AuthorCreatedDomainEvent>,
    INotificationHandler<AuthorUpdatedDomainEvent>,
    INotificationHandler<AuthorDeletedDomainEvent>
{
    public ValueTask HandleAsync(AuthorCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        factory.Events.Enqueue(notification);
        return ValueTask.CompletedTask;
    }

    public ValueTask HandleAsync(AuthorUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        factory.Events.Enqueue(notification);
        return ValueTask.CompletedTask;
    }

    public ValueTask HandleAsync(AuthorDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        factory.Events.Enqueue(notification);
        return ValueTask.CompletedTask;
    }
}
