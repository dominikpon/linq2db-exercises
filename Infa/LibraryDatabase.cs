using LinqToDB;
using LinqToDB.Data;

namespace Infa;

public class LibraryDatabase(DataOptions<LibraryDatabase> dataopts) : DataConnection(dataopts.Options)
{
    public ITable<Author> Authors() => this.GetTable<Author>();
    public ITable<Book> Books() => this.GetTable<Book>();
    public ITable<AuthorBook> AuthorBooks() => this.GetTable<AuthorBook>();
}
