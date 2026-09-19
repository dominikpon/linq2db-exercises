using System.ComponentModel.DataAnnotations;
using API.Dtos;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace API.Controllers;

public partial class BooksController
{
    /// <summary>Adds a new book to the catalogue. It always starts in print.</summary>
    /// <remarks>
    ///     Validation rules, all of them <see cref="ValidationException" />: title is required; price
    ///     is not negative; ISBN is null or exactly 13 digits.
    /// </remarks>
    /// <exception cref="ValidationException">Any rule above is broken.</exception>
    /// <exception cref="InvalidOperationException">Another book already carries the same ISBN.</exception>
    [HttpPost(nameof(Create))]
    public BookResponse Create([FromBody] BookCreateRequest request)
    {
        ValidateTitle(request.Title);
        ValidatePrice(request.PriceDkk);
        ValidateIsbn(request.Isbn);
        EnsureIsbnIsFree(request.Isbn, null);

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Isbn = request.Isbn,
            Genre = request.Genre,
            PriceDkk = request.PriceDkk,
            IsOutOfPrint = false,
            PublishedDate = request.PublishedDate,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Insert(book);
        return new BookResponse(book);
    }

    /// <summary>
    ///     Replaces the columns the request actually supplies; anything left out is left alone.
    /// </summary>
    /// <remarks>
    ///     Every property on the request except the id is optional: <c>null</c> means "leave this
    ///     alone". There is no way to null a nullable column through this endpoint. Idempotent: sending
    ///     the same request twice leaves the same row state.
    /// </remarks>
    /// <exception cref="ValidationException">A validation rule is broken.</exception>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    /// <exception cref="InvalidOperationException">Another book already carries the same ISBN.</exception>
    [HttpPut(nameof(Update))]
    public BookResponse Update([FromBody] BookUpdateRequest request)
    {
        var existing = db.Books().FirstOrDefault(b => b.Id == request.Id)
                       ?? throw new KeyNotFoundException("that book does not exist");

        if (request.Title != null)
        {
            ValidateTitle(request.Title);
            existing.Title = request.Title;
        }

        if (request.Genre != null)
            existing.Genre = request.Genre.Value;

        if (request.PriceDkk != null)
        {
            ValidatePrice(request.PriceDkk.Value);
            existing.PriceDkk = request.PriceDkk.Value;
        }

        if (request.IsOutOfPrint != null)
            existing.IsOutOfPrint = request.IsOutOfPrint.Value;

        if (request.Isbn != null)
        {
            ValidateIsbn(request.Isbn);
            EnsureIsbnIsFree(request.Isbn, existing.Id);
            existing.Isbn = request.Isbn;
        }

        if (request.PublishedDate != null)
            existing.PublishedDate = request.PublishedDate;

        db.Update(existing);
        return new BookResponse(existing);
    }

    /// <summary>Removes a book for good — but only once no author is still credited on it.</summary>
    /// <exception cref="KeyNotFoundException">No row has that id, including when it was already deleted.</exception>
    /// <exception cref="InvalidOperationException">At least one author is still credited on this book.</exception>
    [HttpDelete(nameof(Delete))]
    public void Delete([FromQuery] Guid id)
    {
        var book = db.Books().FirstOrDefault(b => b.Id == id) ??
                   throw new KeyNotFoundException("that book does not exist");

        if (db.AuthorBooks().Any(l => l.BookId == id))
            throw new InvalidOperationException("unlink this book's authors first");

        db.Delete(book);
    }

    /// <summary>
    ///     Takes a book out of print. Idempotent: marking an already out-of-print book is a no-op,
    ///     not an error.
    /// </summary>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    [HttpPost(nameof(MarkOutOfPrint))]
    public void MarkOutOfPrint([FromQuery] Guid id)
    {
        var book = db.Books().FirstOrDefault(b => b.Id == id) ??
                   throw new KeyNotFoundException("that book does not exist");
        book.IsOutOfPrint = true;
        db.Update(book);
    }

    /// <summary>Puts an out-of-print book back in print. Idempotent, exactly like <see cref="MarkOutOfPrint" />.</summary>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    [HttpPost(nameof(MarkInPrint))]
    public void MarkInPrint([FromQuery] Guid id)
    {
        var book = db.Books().FirstOrDefault(b => b.Id == id) ??
                   throw new KeyNotFoundException("that book does not exist");
        book.IsOutOfPrint = false;
        db.Update(book);
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("title cannot be blank");
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
            throw new ValidationException("price cannot be negative");
    }

    private static void ValidateIsbn(string? isbn)
    {
        if (isbn != null && (isbn.Length != 13 || !isbn.All(char.IsDigit)))
            throw new ValidationException("isbn must be null or exactly 13 digits");
    }

    private void EnsureIsbnIsFree(string? isbn, Guid? excluding)
    {
        if (isbn == null) return;
        if (db.Books().Any(b => b.Isbn == isbn && b.Id != excluding))
            throw new InvalidOperationException("that isbn is already taken");
    }

    #region Tests: Create

    public class CreateTests : LibraryTest
    {
        private static BookCreateRequest Sample()
        {
            return new BookCreateRequest(
                "Test Book",
                "1112223334445",
                Genre.Fiction,
                99.00m,
                new DateOnly(2025, 1, 1));
        }

        [Fact]
        public void Inserts_the_book_and_returns_it()
        {
            var created = BooksController.Create(Sample());
            Assert.Equal(14, BookCount);
            Assert.Equal("Test Book", BookRow(created.Id).Title);
        }

        [Fact]
        public void Always_starts_in_print()
        {
            var created = BooksController.Create(Sample());
            Assert.False(BookRow(created.Id).IsOutOfPrint);
        }

        [Fact]
        public void Rejects_a_blank_title()
        {
            Assert.Throws<ValidationException>(() => BooksController.Create(Sample() with { Title = " " }));
        }

        [Fact]
        public void Rejects_a_negative_price()
        {
            Assert.Throws<ValidationException>(() => BooksController.Create(Sample() with { PriceDkk = -1m }));
        }

        [Fact]
        public void Rejects_a_malformed_isbn()
        {
            Assert.Throws<ValidationException>(() => BooksController.Create(Sample() with { Isbn = "123" }));
            Assert.Throws<ValidationException>(() => BooksController.Create(Sample() with { Isbn = "111222333444X" }));
        }

        [Fact]
        public void Rejects_an_isbn_that_is_already_taken()
        {
            Assert.Throws<InvalidOperationException>(() =>
                BooksController.Create(Sample() with { Isbn = "9788700000011" }));
        }
    }

    #endregion

    #region Tests: Update

    public class UpdateTests : LibraryTest
    {
        [Fact]
        public void Changes_only_the_supplied_columns()
        {
            var before = BookRow(LibrarySeed.BookIdOf(1));
            BooksController.Update(new BookUpdateRequest { Id = before.Id, Title = "Renamed" });

            var after = BookRow(before.Id);
            Assert.Equal("Renamed", after.Title);
            Assert.Equal(before.PriceDkk, after.PriceDkk);
            Assert.Equal(before.Isbn, after.Isbn);
        }

        [Fact]
        public void Sets_a_nullable_column()
        {
            var id = LibrarySeed.BookIdOf(3);
            Assert.Null(BookRow(id).Isbn);

            BooksController.Update(new BookUpdateRequest { Id = id, Isbn = "1112223334445" });

            Assert.Equal("1112223334445", BookRow(id).Isbn);
        }

        [Fact]
        public void Leaves_a_nullable_column_alone_when_not_mentioned()
        {
            var id = LibrarySeed.BookIdOf(1);
            var before = BookRow(id).Isbn;

            BooksController.Update(new BookUpdateRequest { Id = id, Title = "Renamed" });

            Assert.Equal(before, BookRow(id).Isbn);
        }

        [Fact]
        public void Is_idempotent()
        {
            var request = new BookUpdateRequest { Id = LibrarySeed.BookIdOf(1), Title = "Renamed" };
            BooksController.Update(request);
            var first = BookRow(request.Id);
            BooksController.Update(request);
            var second = BookRow(request.Id);

            Assert.Equal(first.Title, second.Title);
            Assert.Equal(13, BookCount);
        }

        [Fact]
        public void Throws_when_the_row_does_not_exist()
        {
            Assert.Throws<KeyNotFoundException>(() =>
                BooksController.Update(new BookUpdateRequest { Id = Guid.NewGuid(), Title = "Nope" }));
        }

        [Fact]
        public void Throws_when_the_isbn_belongs_to_another_book()
        {
            var id = LibrarySeed.BookIdOf(1);
            Assert.Throws<InvalidOperationException>(() =>
                BooksController.Update(new BookUpdateRequest { Id = id, Isbn = "9788700000028" }));
        }

        [Fact]
        public void Accepts_its_own_isbn_unchanged()
        {
            var id = LibrarySeed.BookIdOf(1);
            BooksController.Update(new BookUpdateRequest { Id = id, Isbn = "9788700000011" });
            Assert.Equal("9788700000011", BookRow(id).Isbn);
        }
    }

    #endregion

    #region Tests: Delete

    public class DeleteTests : LibraryTest
    {
        [Fact]
        public void Removes_a_book_with_no_authors()
        {
            var id = LibrarySeed.BookIdOf(12);
            BooksController.Delete(id);
            Assert.Equal(12, BookCount);
            Assert.False(BookExists(id));
        }

        [Fact]
        public void Refuses_while_an_author_is_still_credited()
        {
            Assert.Throws<InvalidOperationException>(() => BooksController.Delete(LibrarySeed.BookIdOf(1)));
            Assert.Equal(13, BookCount);
        }

        [Fact]
        public void Deleting_twice_reports_it_is_gone()
        {
            var id = LibrarySeed.BookIdOf(12);
            BooksController.Delete(id);
            Assert.Throws<KeyNotFoundException>(() => BooksController.Delete(id));
        }
    }

    #endregion

    #region Tests: MarkOutOfPrint

    public class MarkOutOfPrintTests : LibraryTest
    {
        [Fact]
        public void Flags_the_row()
        {
            var id = LibrarySeed.BookIdOf(1);
            BooksController.MarkOutOfPrint(id);
            Assert.True(BookRow(id).IsOutOfPrint);
        }

        [Fact]
        public void Doing_it_twice_is_fine()
        {
            var id = LibrarySeed.BookIdOf(1);
            BooksController.MarkOutOfPrint(id);
            BooksController.MarkOutOfPrint(id);
            Assert.True(BookRow(id).IsOutOfPrint);
        }

        [Fact]
        public void Throws_for_an_unknown_id()
        {
            Assert.Throws<KeyNotFoundException>(() => BooksController.MarkOutOfPrint(Guid.NewGuid()));
        }
    }

    #endregion

    #region Tests: MarkInPrint

    public class MarkInPrintTests : LibraryTest
    {
        [Fact]
        public void Brings_the_row_back()
        {
            var id = LibrarySeed.BookIdOf(3);
            BooksController.MarkInPrint(id);
            Assert.False(BookRow(id).IsOutOfPrint);
        }

        [Fact]
        public void Doing_it_twice_is_fine()
        {
            var id = LibrarySeed.BookIdOf(3);
            BooksController.MarkInPrint(id);
            BooksController.MarkInPrint(id);
            Assert.False(BookRow(id).IsOutOfPrint);
        }
    }

    #endregion
}