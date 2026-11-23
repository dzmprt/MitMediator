using Books.Application.Behaviors;
using Books.Application.UseCase.Authors;
using Books.Application.UseCase.Authors.Commands.CreateAuthor;
using Books.Application.UseCase.Authors.Commands.DeleteAuthor;
using Books.Application.UseCase.Authors.Commands.UpdateAuthor;
using Books.Application.UseCase.Authors.Queries.GetAuthor;
using Books.Application.UseCase.Authors.Queries.GetAuthorsByFilter;
using Books.Application.UseCase.Books;
using Books.Application.UseCase.Books.Commands.CreateBook;
using Books.Application.UseCase.Books.Commands.DeleteBook;
using Books.Application.UseCase.Books.Commands.UpdateBook;
using Books.Application.UseCase.Books.Queries.GetBook;
using Books.Application.UseCase.Books.Queries.GetBooksByFilter;
using Books.Application.UseCase.Genres.Commands.CreateGenre;
using Books.Application.UseCase.Genres.Commands.DeleteGenre;
using Books.Domain;
using Books.Domain.Events;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MitMediator;
using Moq;

namespace Books.Application.Tests;

public sealed class EventAndValidationTests
{
    [Fact]
    public async Task AuthorAndBookEventHandlers_ProcessEveryEvent()
    {
        var author = new Author("Ada", "Lovelace");
        var book = new Book("Notes", author, new Genre("Science"));
        var authorHandler = new AuthorDomainEventHandler(NullLogger<AuthorDomainEventHandler>.Instance);
        var bookHandler = new BookDomainEventHandler(NullLogger<BookDomainEventHandler>.Instance);

        await authorHandler.HandleAsync(new AuthorCreatedDomainEvent(author), CancellationToken.None);
        await authorHandler.HandleAsync(new AuthorUpdatedDomainEvent(author), CancellationToken.None);
        await authorHandler.HandleAsync(new AuthorDeletedDomainEvent(1), CancellationToken.None);
        await bookHandler.HandleAsync(new BookCreatedDomainEvent(book), CancellationToken.None);
        await bookHandler.HandleAsync(new BookUpdatedDomainEvent(book), CancellationToken.None);
        await bookHandler.HandleAsync(new BookDeletedDomainEvent(1), CancellationToken.None);
    }

    [Fact]
    public void AddApplicationServices_RegistersValidationPipelineAndValidators()
    {
        var services = new ServiceCollection();

        services.AddApplicationServices();
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(INotificationHandler<AuthorCreatedDomainEvent>));
        using var provider = services.BuildServiceProvider();

        Assert.NotEmpty(provider.GetServices<IValidator<CreateAuthorCommand>>());
        Assert.NotEmpty(provider.GetServices<IPipelineBehavior<CreateAuthorCommand, Author>>());
    }

    [Fact]
    public void Validators_RejectInvalidAndAcceptValidCommands()
    {
        AssertInvalid(new CreateAuthorCommandValidator(), new CreateAuthorCommand { FirstName = "", LastName = "" });
        AssertInvalid(new UpdateAuthorCommandValidator(), new UpdateAuthorCommand { FirstName = "", LastName = "" });
        AssertInvalid(new DeleteAuthorCommandValidator(), new DeleteAuthorCommand());
        AssertInvalid(new GetAuthorQueryValidator(), new GetAuthorQuery());
        AssertInvalid(new GetAuthorsByFilterQueryValidator(), new GetAuthorsByFilterQuery { Limit = -1, Offset = -1, FreeText = new string('a', 1001) });
        AssertInvalid(new CreateBookCommandValidator(), new CreateBookCommand { Title = "", GenreName = "" });
        AssertInvalid(new UpdateBookCommandValidator(), new UpdateBookCommand { Title = "", GenreName = "" });
        AssertInvalid(new DeleteBookCommandValidator(), new DeleteBookCommand());
        AssertInvalid(new GetBookQueryValidator(), new GetBookQuery());
        AssertInvalid(new GetBooksByFilterQueryValidator(), new GetBooksByFilterQuery { Limit = -1, Offset = -1, FreeText = new string('a', 1001) });
        AssertInvalid(new CreateGenreCommandValidator(), new CreateGenreCommand { GenreName = "" });
        AssertInvalid(new DeleteGenreCommandValidator(), new DeleteGenreCommand { GenreName = "" });

        AssertValid(new CreateAuthorCommandValidator(), new CreateAuthorCommand { FirstName = "Ada", LastName = "Lovelace" });
        AssertValid(new CreateBookCommandValidator(), new CreateBookCommand { Title = "Notes", AuthorId = 1, GenreName = "Science" });
        AssertValid(new CreateGenreCommandValidator(), new CreateGenreCommand { GenreName = "Science" });
    }

    [Fact]
    public async Task ValidationBehavior_InvokesNextOrThrows()
    {
        var passingValidator = new InlineValidator<CreateAuthorCommand>();
        passingValidator.RuleFor(x => x.FirstName).NotEmpty();
        var passingServices = new ServiceCollection()
            .AddSingleton<IValidator<CreateAuthorCommand>>(passingValidator)
            .BuildServiceProvider();
        var behavior = new ValidationBehavior<CreateAuthorCommand, Author>(passingServices);
        var next = new Mock<IRequestHandlerNext<CreateAuthorCommand, Author>>();
        var author = new Author("Ada", "Lovelace");
        next
            .Setup(x => x.InvokeAsync(It.IsAny<CreateAuthorCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Author>(author));

        var result = await behavior.HandleAsync(
            new CreateAuthorCommand { FirstName = "Ada", LastName = "Lovelace" },
            next.Object,
            CancellationToken.None);
        Assert.Same(author, result);

        var failingValidator = new InlineValidator<CreateAuthorCommand>();
        failingValidator.RuleFor(x => x.FirstName).NotEmpty();
        var failingServices = new ServiceCollection()
            .AddSingleton<IValidator<CreateAuthorCommand>>(failingValidator)
            .BuildServiceProvider();
        var failingBehavior = new ValidationBehavior<CreateAuthorCommand, Author>(failingServices);

        await Assert.ThrowsAsync<ValidationException>(async () =>
            await failingBehavior.HandleAsync(
                new CreateAuthorCommand { FirstName = "", LastName = "Lovelace" },
                next.Object,
                CancellationToken.None));
    }

    private static void AssertInvalid<T>(IValidator<T> validator, T value)
        => Assert.False(validator.Validate(value).IsValid);

    private static void AssertValid<T>(IValidator<T> validator, T value)
        => Assert.True(validator.Validate(value).IsValid);
}
