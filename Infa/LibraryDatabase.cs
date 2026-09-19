using LinqToDB;
using LinqToDB.Data;

namespace Infa;

public class LibraryDatabase(DataOptions<LibraryDatabase> dataopts) : DataConnection(dataopts.Options)
{
    public ITable<Author> Authors()
    {
        return this.GetTable<Author>();
    }

    public ITable<Book> Books()
    {
        return this.GetTable<Book>();
    }

    public ITable<AuthorBook> AuthorBooks()
    {
        return this.GetTable<AuthorBook>();
    }
}