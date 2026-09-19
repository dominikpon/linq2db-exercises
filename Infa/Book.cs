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
    [PrimaryKey] public Guid Id { get; set; }

    [Column, NotNull] public string Title { get; set; } = "";
    [Column] public string? Isbn { get; set; }
    [Column, NotNull] public Genre Genre { get; set; }
    [Column] public decimal PriceDkk { get; set; }
    [Column] public bool IsOutOfPrint { get; set; }
    [Column] public DateOnly? PublishedDate { get; set; }
    [Column] public DateTime CreatedAtUtc { get; set; }
}
