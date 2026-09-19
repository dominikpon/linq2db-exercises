using System.ComponentModel.DataAnnotations;
using API.Dtos;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace API.Controllers;

public partial class AuthorsController
{
    /// <summary>Adds a new author to the catalogue.</summary>
    /// <remarks>
    ///     Validation rules, all of them <see cref="ValidationException" />: first and last name are
    ///     required; website is null or starts with <c>http://</c>/<c>https://</c>; birth date is
    ///     null or not in the future.
    /// </remarks>
    /// <exception cref="ValidationException">Any rule above is broken.</exception>
    [HttpPost(nameof(Create))]
    public AuthorResponse Create([FromBody] AuthorCreateRequest request)
    {
        ValidateName(request.FirstName, request.LastName);
        ValidateWebsite(request.Website);
        ValidateBirthDate(request.BirthDate);

        var author = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Bio = request.Bio,
            Nationality = request.Nationality,
            Website = request.Website,
            BirthDate = request.BirthDate,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Insert(author);
        return new AuthorResponse(author);
    }

    /// <summary>
    ///     Replaces the columns the request actually supplies; anything left out is left alone.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Every property on the request except the id is optional: <c>null</c> means "leave this
    ///         alone". There is no way to null a nullable column through this endpoint.
    ///     </para>
    ///     <para>
    ///         The same validation rules as <see cref="Create" /> apply to whichever fields are actually
    ///         supplied. This is idempotent: sending the same request twice leaves the same row state and
    ///         throws nothing the second time.
    ///     </para>
    /// </remarks>
    /// <exception cref="ValidationException">A validation rule is broken.</exception>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    [HttpPut(nameof(Update))]
    public AuthorResponse Update([FromBody] AuthorUpdateRequest request)
    {
        var existing = db.Authors().FirstOrDefault(a => a.Id == request.Id)
                       ?? throw new KeyNotFoundException("that author does not exist");

        if (request.FirstName != null)
        {
            ValidateName(request.FirstName, existing.LastName);
            existing.FirstName = request.FirstName;
        }

        if (request.LastName != null)
        {
            ValidateName(existing.FirstName, request.LastName);
            existing.LastName = request.LastName;
        }

        if (request.Bio != null)
            existing.Bio = request.Bio;

        if (request.Nationality != null)
            existing.Nationality = request.Nationality;

        if (request.Website != null)
        {
            ValidateWebsite(request.Website);
            existing.Website = request.Website;
        }

        if (request.BirthDate != null)
        {
            ValidateBirthDate(request.BirthDate);
            existing.BirthDate = request.BirthDate;
        }

        db.Update(existing);
        return new AuthorResponse(existing);
    }

    /// <summary>Removes an author for good — but only once nothing they wrote is still credited to them.</summary>
    /// <exception cref="KeyNotFoundException">No row has that id, including when it was already deleted.</exception>
    /// <exception cref="InvalidOperationException">The author is still credited on at least one book.</exception>
    [HttpDelete(nameof(Delete))]
    public void Delete([FromQuery] Guid id)
    {
        var author = db.Authors().FirstOrDefault(a => a.Id == id) ??
                     throw new KeyNotFoundException("that author does not exist");

        if (db.AuthorBooks().Any(l => l.AuthorId == id))
            throw new InvalidOperationException("unlink this author's books first");

        db.Delete(author);
    }

    private static void ValidateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ValidationException("first name cannot be blank");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ValidationException("last name cannot be blank");
    }

    private static void ValidateWebsite(string? website)
    {
        if (website != null && !website.StartsWith("http://") && !website.StartsWith("https://"))
            throw new ValidationException("website must start with http:// or https://");
    }

    private static void ValidateBirthDate(DateOnly? birthDate)
    {
        if (birthDate != null && birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ValidationException("birth date cannot be in the future");
    }

    #region Tests: Create

    public class CreateTests : LibraryTest
    {
        private static AuthorCreateRequest Sample()
        {
            return new AuthorCreateRequest(
                "Test",
                "Writer",
                "A brand new author.",
                "Danish",
                "https://example.com",
                new DateOnly(1990, 1, 1));
        }

        [Fact]
        public void Inserts_the_author_and_returns_it()
        {
            var created = AuthorsController.Create(Sample());
            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Equal(12, AuthorCount);
            Assert.Equal("Test", AuthorRow(created.Id).FirstName);
        }

        [Fact]
        public void Stamps_the_creation_time()
        {
            var created = AuthorsController.Create(Sample());
            Assert.InRange(AuthorRow(created.Id).CreatedAtUtc, DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(1));
        }

        [Fact]
        public void Rejects_a_blank_name()
        {
            Assert.Throws<ValidationException>(() => AuthorsController.Create(Sample() with { FirstName = "  " }));
            Assert.Throws<ValidationException>(() => AuthorsController.Create(Sample() with { LastName = "" }));
        }

        [Fact]
        public void Rejects_a_malformed_website()
        {
            Assert.Throws<ValidationException>(() =>
                AuthorsController.Create(Sample() with { Website = "example.com" }));
        }

        [Fact]
        public void Rejects_a_birth_date_in_the_future()
        {
            Assert.Throws<ValidationException>(() =>
                AuthorsController.Create(
                    Sample() with { BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) }));
        }
    }

    #endregion

    #region Tests: Update

    public class UpdateTests : LibraryTest
    {
        [Fact]
        public void Changes_only_the_supplied_columns()
        {
            var before = AuthorRow(LibrarySeed.AuthorIdOf(1));
            AuthorsController.Update(new AuthorUpdateRequest { Id = before.Id, FirstName = "Renamed" });

            var after = AuthorRow(before.Id);
            Assert.Equal("Renamed", after.FirstName);
            Assert.Equal(before.LastName, after.LastName);
            Assert.Equal(before.Nationality, after.Nationality);
        }

        [Fact]
        public void Sets_a_nullable_column()
        {
            var id = LibrarySeed.AuthorIdOf(2);
            Assert.Null(AuthorRow(id).Website);

            AuthorsController.Update(new AuthorUpdateRequest { Id = id, Website = "https://tobias.example.com" });

            Assert.Equal("https://tobias.example.com", AuthorRow(id).Website);
        }

        [Fact]
        public void Leaves_a_nullable_column_alone_when_not_mentioned()
        {
            var id = LibrarySeed.AuthorIdOf(1);
            var before = AuthorRow(id).Website;

            AuthorsController.Update(new AuthorUpdateRequest { Id = id, FirstName = "Renamed" });

            Assert.Equal(before, AuthorRow(id).Website);
        }

        [Fact]
        public void Is_idempotent()
        {
            var request = new AuthorUpdateRequest { Id = LibrarySeed.AuthorIdOf(1), FirstName = "Renamed" };
            AuthorsController.Update(request);
            var first = AuthorRow(request.Id);
            AuthorsController.Update(request);
            var second = AuthorRow(request.Id);

            Assert.Equal(first.FirstName, second.FirstName);
            Assert.Equal(11, AuthorCount);
        }

        [Fact]
        public void Throws_when_the_row_does_not_exist()
        {
            Assert.Throws<KeyNotFoundException>(() =>
                AuthorsController.Update(new AuthorUpdateRequest { Id = Guid.NewGuid(), FirstName = "Nope" }));
        }

        [Fact]
        public void Throws_when_a_supplied_field_is_invalid()
        {
            var id = LibrarySeed.AuthorIdOf(1);
            Assert.Throws<ValidationException>(() =>
                AuthorsController.Update(new AuthorUpdateRequest { Id = id, Website = "not-a-url" }));
            Assert.Equal("Elena", AuthorRow(id).FirstName);
        }
    }

    #endregion

    #region Tests: Delete

    public class DeleteTests : LibraryTest
    {
        [Fact]
        public void Removes_an_author_with_no_books()
        {
            var id = LibrarySeed.AuthorIdOf(11);
            AuthorsController.Delete(id);
            Assert.Equal(10, AuthorCount);
            Assert.False(AuthorExists(id));
        }

        [Fact]
        public void Refuses_while_a_book_is_still_credited()
        {
            Assert.Throws<InvalidOperationException>(() => AuthorsController.Delete(LibrarySeed.AuthorIdOf(1)));
            Assert.Equal(11, AuthorCount);
        }

        [Fact]
        public void Deleting_twice_reports_it_is_gone()
        {
            var id = LibrarySeed.AuthorIdOf(11);
            AuthorsController.Delete(id);
            Assert.Throws<KeyNotFoundException>(() => AuthorsController.Delete(id));
        }
    }

    #endregion
}