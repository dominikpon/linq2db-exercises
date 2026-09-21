using Infa;

namespace API.Dtos;

/// <summary>A book plus its authors. Load <see cref="Book.Authors" /> before constructing.</summary>
public record BookWithAuthorsResponse : BookResponse
{
    public BookWithAuthorsResponse(Book book) : base(book)
    {
        Authors = book.Authors.Select(a => new AuthorResponse(a)).ToList();
    }

    public List<AuthorResponse> Authors { get; init; }
}

/// <summary>An author plus their books. Load <see cref="Author.Books" /> before constructing.</summary>
public record AuthorWithBooksResponse : AuthorResponse
{
    public AuthorWithBooksResponse(Author author) : base(author)
    {
        Books = author.Books.Select(b => new BookResponse(b)).ToList();
    }

    public List<BookResponse> Books { get; init; }
}
