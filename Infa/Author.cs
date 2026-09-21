using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.Mapping;

namespace Infa;

[Table("Authors")]
public class Author
{
    [PrimaryKey] public string Id { get; set; } = "";

    [Column] [NotNull] public string FirstName { get; set; } = "";
    [Column] [NotNull] public string LastName { get; set; } = "";
    [Column] public string? Bio { get; set; }
    [Column] public string? Nationality { get; set; }
    [Column] public string? Website { get; set; }
    [Column] public DateOnly? BirthDate { get; set; }
    [Column] [ValueConverter(ConverterType = typeof(UtcDateTimeConverter))] public DateTime CreatedAtUtc { get; set; }

    /// <summary>Not a column: the books credited to this author, reached through <see cref="AuthorBook" />. Fill it with <c>LoadWith</c>.</summary>
    [Association(QueryExpressionMethod = nameof(BooksExpression))]
    public IEnumerable<Book> Books { get; set; } = [];

    public static Expression<Func<Author, IDataContext, IQueryable<Book>>> BooksExpression()
    {
        return (author, ctx) => from link in ctx.GetTable<AuthorBook>()
            where link.AuthorId == author.Id
            join book in ctx.GetTable<Book>() on link.BookId equals book.Id
            select book;
    }
}