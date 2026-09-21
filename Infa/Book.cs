using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.Mapping;

namespace Infa;

public enum Genre
{
    [MapValue("fiction")] Fiction,
    [MapValue("non_fiction")] NonFiction,
    [MapValue("sci_fi")] SciFi,
    [MapValue("fantasy")] Fantasy,
    [MapValue("mystery")] Mystery,
    [MapValue("biography")] Biography
}

[Table("Books")]
public class Book
{
    [PrimaryKey] public string Id { get; set; } = "";

    [Column] [NotNull] public string Title { get; set; } = "";
    [Column] public string? Isbn { get; set; }
    [Column] [NotNull] public Genre Genre { get; set; }
    [Column] public decimal PriceDkk { get; set; }
    [Column] public bool IsOutOfPrint { get; set; }
    [Column] public DateOnly? PublishedDate { get; set; }
    [Column] [ValueConverter(ConverterType = typeof(UtcDateTimeConverter))] public DateTime CreatedAtUtc { get; set; }

    /// <summary>Not a column: the authors credited on this book, reached through <see cref="AuthorBook" />. Fill it with <c>LoadWith</c>.</summary>
    [Association(QueryExpressionMethod = nameof(AuthorsExpression))]
    public IEnumerable<Author> Authors { get; set; } = [];

    public static Expression<Func<Book, IDataContext, IQueryable<Author>>> AuthorsExpression()
    {
        return (book, ctx) => from link in ctx.GetTable<AuthorBook>()
            where link.BookId == book.Id
            join author in ctx.GetTable<Author>() on link.AuthorId equals author.Id
            select author;
    }
}