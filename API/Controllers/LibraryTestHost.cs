using API.Controllers;
using Infa;
using LinqToDB;

namespace API.Testing;

public sealed class LibraryTestHost : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"library-tests-{Guid.NewGuid():N}.db");
    private readonly IServiceScope _scope;

    public LibraryTestHost()
    {
        var options = new DataOptions().UseSQLite($"Data Source={_path};Pooling=False");

        var services = new ServiceCollection();
        services.AddSingleton(new DataOptions<LibraryDatabase>(options));
        services.AddScoped<LibraryDatabase>();
        services.AddScoped<AuthorsController>();
        services.AddScoped<BooksController>();
        services.AddScoped<AuthorBooksController>();

        _scope = services.BuildServiceProvider().CreateScope();
        LibrarySeed.EnsureSeeded(Db);
    }

    public LibraryDatabase Db => _scope.ServiceProvider.GetRequiredService<LibraryDatabase>();
    public AuthorsController Authors => _scope.ServiceProvider.GetRequiredService<AuthorsController>();
    public BooksController Books => _scope.ServiceProvider.GetRequiredService<BooksController>();
    public AuthorBooksController AuthorBooks => _scope.ServiceProvider.GetRequiredService<AuthorBooksController>();

    public void Dispose()
    {
        _scope.Dispose();
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

    protected Author AuthorRow(Guid id) => Db.Authors().Single(x => x.Id == id);
    protected Book BookRow(Guid id) => Db.Books().Single(x => x.Id == id);
    protected bool AuthorExists(Guid id) => Db.Authors().Any(x => x.Id == id);
    protected bool BookExists(Guid id) => Db.Books().Any(x => x.Id == id);
    protected bool IsLinked(Guid authorId, Guid bookId) => Db.AuthorBooks().Any(x => x.AuthorId == authorId && x.BookId == bookId);

    public void Dispose() => _host.Dispose();
}
