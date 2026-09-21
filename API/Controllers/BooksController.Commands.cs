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
    ///     Validation rules, both a <see cref="ValidationException" />: the title is required (not
    ///     null, empty or whitespace) and the price is not negative.
    /// </remarks>
    /// <exception cref="ValidationException">The title is blank or the price is negative.</exception>
    [HttpPost(nameof(Create))]
    public BookResponse Create([FromBody] BookCreateRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Changes only the columns the request actually supplies; anything left out is left alone.
    ///     This is the <c>PATCH</c> half of updating: to set a nullable column back to <c>null</c>, use
    ///     <see cref="Replace" />.
    /// </summary>
    /// <remarks>
    ///     Every property on the request except the id is optional: <c>null</c> means "leave this
    ///     alone". There is no way to null a nullable column through this endpoint. Idempotent: sending
    ///     the same request twice leaves the same row state.
    /// </remarks>
    /// <exception cref="ValidationException">A validation rule is broken.</exception>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    [HttpPatch(nameof(Update))]
    public BookResponse Update([FromBody] BookUpdateRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Replaces the whole mutable row with the request. Every column is overwritten with what was
    ///     sent, nulls included, so this is also how a nullable column is cleared.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The row is found by <see cref="BookReplaceRequest.Id" />. <see cref="Book.Id" /> and
    ///         <see cref="Book.CreatedAtUtc" /> are never changed. The validation rules of
    ///         <see cref="Create" /> apply to the values sent; unlike on create, <c>IsOutOfPrint</c> is
    ///         taken from the request.
    ///     </para>
    ///     <para>Idempotent: sending the same request twice leaves the same row state.</para>
    /// </remarks>
    /// <exception cref="ValidationException">A validation rule is broken.</exception>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    [HttpPut(nameof(Replace))]
    public BookResponse Replace([FromBody] BookReplaceRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>Removes a book for good — but only once no author is still credited on it.</summary>
    /// <exception cref="KeyNotFoundException">No row has that id, including when it was already deleted.</exception>
    /// <exception cref="InvalidOperationException">At least one author is still credited on this book.</exception>
    [HttpDelete(nameof(Delete))]
    public void Delete([FromQuery] string id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Takes a book out of print. Idempotent: marking an already out-of-print book is a no-op,
    ///     not an error.
    /// </summary>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    [HttpPost(nameof(MarkOutOfPrint))]
    public void MarkOutOfPrint([FromQuery] string id)
    {
        throw new NotImplementedException();
    }

    /// <summary>Puts an out-of-print book back in print. Idempotent, exactly like <see cref="MarkOutOfPrint" />.</summary>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    [HttpPost(nameof(MarkInPrint))]
    public void MarkInPrint([FromQuery] string id)
    {
        throw new NotImplementedException();
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
        public void Throws_when_the_row_does_not_exist()
        {
            Assert.Throws<KeyNotFoundException>(() =>
                BooksController.Update(new BookUpdateRequest { Id = Guid.NewGuid().ToString(), Title = "Nope" }));
        }

    }

    #endregion

    #region Tests: Replace

    public class ReplaceTests : LibraryTest
    {
        private static BookReplaceRequest Full(string id)
        {
            return new BookReplaceRequest(
                id,
                "New Title",
                "1112223334445",
                Genre.Mystery,
                55.5m,
                true,
                new DateOnly(2001, 2, 3));
        }

        [Fact]
        public void Replaces_every_column()
        {
            var id = LibrarySeed.BookIdOf(1);
            BooksController.Replace(Full(id));

            var saved = BookRow(id);
            Assert.Equal("New Title", saved.Title);
            Assert.Equal("1112223334445", saved.Isbn);
            Assert.Equal(Genre.Mystery, saved.Genre);
            Assert.Equal(55.5m, saved.PriceDkk);
            Assert.True(saved.IsOutOfPrint);
            Assert.Equal(new DateOnly(2001, 2, 3), saved.PublishedDate);
        }

        [Fact]
        public void Clears_nullable_columns_sent_as_null()
        {
            var id = LibrarySeed.BookIdOf(1);
            Assert.NotNull(BookRow(id).Isbn);

            BooksController.Replace(Full(id) with { Isbn = null, PublishedDate = null });

            var saved = BookRow(id);
            Assert.Null(saved.Isbn);
            Assert.Null(saved.PublishedDate);
        }

        [Fact]
        public void Is_idempotent()
        {
            var id = LibrarySeed.BookIdOf(1);
            BooksController.Replace(Full(id));
            var first = BookRow(id);
            BooksController.Replace(Full(id));
            var second = BookRow(id);

            Assert.Equal(first.Title, second.Title);
            Assert.Equal(first.Isbn, second.Isbn);
            Assert.Equal(13, BookCount);
        }

        [Fact]
        public void Throws_when_the_row_does_not_exist()
        {
            Assert.Throws<KeyNotFoundException>(() => BooksController.Replace(Full(Guid.NewGuid().ToString())));
        }

        [Fact]
        public void Throws_when_a_value_is_invalid_and_changes_nothing()
        {
            var id = LibrarySeed.BookIdOf(1);
            Assert.Throws<ValidationException>(() => BooksController.Replace(Full(id) with { Title = " " }));
            Assert.Throws<ValidationException>(() => BooksController.Replace(Full(id) with { PriceDkk = -1m }));
            Assert.Equal("The Glass Meridian", BookRow(id).Title);
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
            Assert.Throws<KeyNotFoundException>(() => BooksController.MarkOutOfPrint(Guid.NewGuid().ToString()));
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
