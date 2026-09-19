using LinqToDB.Mapping;

namespace Infa;

[Table("Authors")]
public class Author
{
    [PrimaryKey] public Guid Id { get; set; }

    [Column] [NotNull] public string FirstName { get; set; } = "";
    [Column] [NotNull] public string LastName { get; set; } = "";
    [Column] public string? Bio { get; set; }
    [Column] public string? Nationality { get; set; }
    [Column] public string? Website { get; set; }
    [Column] public DateOnly? BirthDate { get; set; }
    [Column] public DateTime CreatedAtUtc { get; set; }
}