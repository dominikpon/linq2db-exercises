using Facet;
using Infa;

namespace API.Dtos;

[Facet(typeof(Author), GenerateToSource = false)]
public partial record AuthorResponse;

[Facet(typeof(Author), [nameof(Author.Id), nameof(Author.CreatedAtUtc)], GenerateToSource = false)]
public partial record AuthorCreateRequest;

/// <summary>Every property except <c>Id</c> is optional: a null one is left alone.</summary>
[Facet(typeof(Author),
    [nameof(Author.Id), nameof(Author.CreatedAtUtc)],
    NullableProperties = true,
    GenerateToSource = false)]
public partial record AuthorUpdateRequest
{
    public Guid Id { get; init; }
}