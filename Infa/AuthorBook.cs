using LinqToDB.Mapping;

namespace Infa;

/// <summary>
///     Junction row for the many-to-many between <see cref="Author" /> and <see cref="Book" />.
///     The pair (<see cref="AuthorId" />, <see cref="BookId" />) is the composite primary key, so an
///     author can only be linked to the same book once.
/// </summary>
[Table("AuthorBooks")]
public class AuthorBook
{
    [PrimaryKey(1)] [Column] public string AuthorId { get; set; } = "";
    [PrimaryKey(2)] [Column] public string BookId { get; set; } = "";
}