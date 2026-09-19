using API.Controllers;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Mvc.Testing;

namespace API;

public sealed class LibraryTestHost : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"library-tests-{Guid.NewGuid():N}.db");
    private readonly IServiceScope _scope;

    public LibraryTestHost()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseSetting("DB", $"Data Source={_path};Pooling=False"));
        _scope = _factory.Services.CreateScope();

        Authors = ActivatorUtilities.CreateInstance<AuthorsController>(_scope.ServiceProvider);
        Books = ActivatorUtilities.CreateInstance<BooksController>(_scope.ServiceProvider);
        AuthorBooks = ActivatorUtilities.CreateInstance<AuthorBooksController>(_scope.ServiceProvider);
    }

    public LibraryDatabase Db => _scope.ServiceProvider.GetRequiredService<LibraryDatabase>();
    public AuthorsController Authors { get; }
    public BooksController Books { get; }
    public AuthorBooksController AuthorBooks { get; }

    public void Dispose()
    {
        _scope.Dispose();
        _factory.Dispose();
        File.Delete(_path);
    }
}

public abstract class LibraryTest : IDisposable
{
    private readonly LibraryTestHost _host = new();

    protected AuthorsController AuthorsController => _host.Authors;
    protected BooksController BooksController => _host.Books;
    protected AuthorBooksController AuthorBooksController => _host.AuthorBooks;
    protected LibraryDatabase Db => _host.Db;

    protected ITable<Author> AuthorRows => Db.Authors();
    protected ITable<Book> BookRows => Db.Books();
    protected ITable<AuthorBook> LinkRows => Db.AuthorBooks();

    protected int AuthorCount => Db.Authors().Count();
    protected int BookCount => Db.Books().Count();

    public void Dispose()
    {
        _host.Dispose();
    }

    protected Author AuthorRow(Guid id)
    {
        return Db.Authors().Single(x => x.Id == id);
    }

    protected Book BookRow(Guid id)
    {
        return Db.Books().Single(x => x.Id == id);
    }

    protected bool AuthorExists(Guid id)
    {
        return Db.Authors().Any(x => x.Id == id);
    }

    protected bool BookExists(Guid id)
    {
        return Db.Books().Any(x => x.Id == id);
    }

    protected bool IsLinked(Guid authorId, Guid bookId)
    {
        return Db.AuthorBooks().Any(x => x.AuthorId == authorId && x.BookId == bookId);
    }
}