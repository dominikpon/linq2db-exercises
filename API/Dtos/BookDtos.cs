using Facet;
using Infa;

namespace API.Dtos;

[Facet(typeof(Book), GenerateToSource = false)]
public partial record BookResponse;

/// <summary>
/// <see cref="Book.IsOutOfPrint"/> is excluded: every new book starts in print, the server decides
/// that, not the caller.
/// </summary>
[Facet(typeof(Book), exclude: [nameof(Book.Id), nameof(Book.CreatedAtUtc), nameof(Book.IsOutOfPrint)], GenerateToSource = false)]
public partial record BookCreateRequest;

/// <summary>Every property except <c>Id</c> is optional: a null one is left alone.</summary>
[Facet(typeof(Book),
    exclude: [nameof(Book.Id), nameof(Book.CreatedAtUtc)],
    NullableProperties = true,
    GenerateToSource = false)]
public partial record BookUpdateRequest
{
    public Guid Id { get; init; }
}
