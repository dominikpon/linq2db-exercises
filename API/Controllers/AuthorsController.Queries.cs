using System.ComponentModel.DataAnnotations;
using API.Dtos;
using API.Enums;
using Infa;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace API.Controllers;

[Route("[controller]")]
public partial class AuthorsController(LibraryDatabase db) : ControllerBase
{
    /// <summary>Every author, ordered by last name then first name.</summary>
    /// <returns>All 11 seeded authors.</returns>
    [HttpGet(nameof(GetAll))]
    public List<AuthorResponse> GetAll()
    {
        return db.Authors()
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Select(AuthorResponse.Projection)
            .ToList();
    }

    /// <summary>One author looked up by primary key.</summary>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    [HttpGet(nameof(GetById))]
    public AuthorResponse GetById([FromQuery] Guid id)
    {
        return db.Authors()
            .Where(a => a.Id == id)
            .Select(AuthorResponse.Projection)
            .FirstOrDefault() ?? throw new KeyNotFoundException("that author does not exist");
    }

    /// <summary>How many authors the table holds.</summary>
    [HttpGet(nameof(Count))]
    public int Count()
    {
        return db.Authors().Count();
    }

    /// <summary>
    ///     Free-text search over <see cref="Author.FirstName" /> and <see cref="Author.LastName" />.
    ///     A partial, case-insensitive match is enough.
    /// </summary>
    /// <param name="q">At least two characters, otherwise the search is meaningless.</param>
    /// <exception cref="ValidationException"><paramref name="q" /> is null, blank or shorter than two characters.</exception>
    [HttpGet(nameof(Search))]
    public List<AuthorResponse> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            throw new ValidationException("search term must be at least two characters");

        var term = q.Trim();
        return db.Authors()
            .Where(a => a.FirstName.Contains(term) || a.LastName.Contains(term))
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Select(AuthorResponse.Projection)
            .ToList();
    }

    /// <summary>One page of authors, ordered by last name then first name.</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="size">Rows per page, at most 100.</param>
    /// <exception cref="ValidationException">
    ///     <paramref name="page" /> is below 1, or <paramref name="size" /> is outside
    ///     1..100.
    /// </exception>
    [HttpGet(nameof(GetPage))]
    public List<AuthorResponse> GetPage([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        if (page < 1)
            throw new ValidationException("page must be at least 1");
        if (size < 1 || size > 100)
            throw new ValidationException("size must be between 1 and 100");

        return db.Authors()
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(AuthorResponse.Projection)
            .ToList();
    }

    /// <summary>
    ///     Every author, sorted by a caller-chosen column. Sorting happens in SQL.
    /// </summary>
    [HttpGet(nameof(GetSorted))]
    public List<AuthorResponse> GetSorted([FromQuery] AuthorSort by, [FromQuery] bool descending = false)
    {
        var q = by switch
        {
            AuthorSort.Name => descending
                ? db.Authors().OrderByDescending(a => a.LastName).ThenByDescending(a => a.FirstName)
                : db.Authors().OrderBy(a => a.LastName).ThenBy(a => a.FirstName),
            AuthorSort.BirthDate => descending
                ? db.Authors().OrderByDescending(a => a.BirthDate)
                : db.Authors().OrderBy(a => a.BirthDate),
            AuthorSort.Created => descending
                ? db.Authors().OrderByDescending(a => a.CreatedAtUtc)
                : db.Authors().OrderBy(a => a.CreatedAtUtc),
            _ => throw new ArgumentOutOfRangeException(nameof(by))
        };

        return q.Select(AuthorResponse.Projection).ToList();
    }

    /// <summary>
    ///     A filtered list. Every parameter that is supplied narrows the result; every parameter left
    ///     null is ignored.
    /// </summary>
    /// <param name="q">Matches first or last name, case-insensitive and partial.</param>
    /// <param name="nationality">Exact match.</param>
    /// <param name="bornAfter">Inclusive lower bound on <see cref="Author.BirthDate" />.</param>
    /// <param name="bornBefore">Inclusive upper bound on <see cref="Author.BirthDate" />.</param>
    /// <returns>Matching authors, ordered by last name. No parameters at all returns everyone.</returns>
    /// <exception cref="ValidationException"><paramref name="bornAfter" /> is later than <paramref name="bornBefore" />.</exception>
    [HttpGet(nameof(GetFiltered))]
    public List<AuthorResponse> GetFiltered(
        [FromQuery] string? q = null,
        [FromQuery] string? nationality = null,
        [FromQuery] DateOnly? bornAfter = null,
        [FromQuery] DateOnly? bornBefore = null)
    {
        if (bornAfter != null && bornBefore != null && bornAfter > bornBefore)
            throw new ValidationException("bornAfter cannot be later than bornBefore");

        var query = db.Authors().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(a => a.FirstName.Contains(q) || a.LastName.Contains(q));
        if (nationality != null)
            query = query.Where(a => a.Nationality == nationality);
        if (bornAfter != null)
            query = query.Where(a => a.BirthDate >= bornAfter);
        if (bornBefore != null)
            query = query.Where(a => a.BirthDate <= bornBefore);

        return query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Select(AuthorResponse.Projection)
            .ToList();
    }

    /// <summary>Every author credited on one book, ordered by last name.</summary>
    /// <exception cref="KeyNotFoundException">No book has that id.</exception>
    [HttpGet(nameof(GetForBook))]
    public List<AuthorResponse> GetForBook([FromQuery] Guid bookId)
    {
        if (!db.Books().Any(b => b.Id == bookId))
            throw new KeyNotFoundException("that book does not exist");

        return db.Authors()
            .Where(a => db.AuthorBooks().Any(l => l.BookId == bookId && l.AuthorId == a.Id))
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Select(AuthorResponse.Projection)
            .ToList();
    }

    /// <summary>Authors with no book credited to them at all.</summary>
    /// <returns>1 author on the seed data.</returns>
    [HttpGet(nameof(GetWithoutBooks))]
    public List<AuthorResponse> GetWithoutBooks()
    {
        return db.Authors()
            .Where(a => !db.AuthorBooks().Any(l => l.AuthorId == a.Id))
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Select(AuthorResponse.Projection)
            .ToList();
    }

    #region Tests: GetAll

    public class GetAllTests : LibraryTest
    {
        [Fact]
        public void Returns_every_seeded_author()
        {
            Assert.Equal(11, AuthorsController.GetAll().Count);
        }

        [Fact]
        public void Is_ordered_by_last_name_then_first_name()
        {
            var names = AuthorsController.GetAll().Select(a => (a.LastName, a.FirstName)).ToList();
            Assert.Equal(
                names.OrderBy(n => n.LastName, StringComparer.Ordinal).ThenBy(n => n.FirstName, StringComparer.Ordinal),
                names);
        }
    }

    #endregion

    #region Tests: GetById

    public class GetByIdTests : LibraryTest
    {
        [Fact]
        public void Returns_the_author_for_a_known_id()
        {
            var author = AuthorsController.GetById(LibrarySeed.AuthorIdOf(1));
            Assert.Equal("Elena", author.FirstName);
            Assert.Equal("Marsh", author.LastName);
        }

        [Fact]
        public void Throws_when_the_id_is_unknown()
        {
            Assert.Throws<KeyNotFoundException>(() => AuthorsController.GetById(Guid.NewGuid()));
        }
    }

    #endregion

    #region Tests: Count

    public class CountTests : LibraryTest
    {
        [Fact]
        public void Counts_the_seeded_rows()
        {
            Assert.Equal(11, AuthorsController.Count());
        }
    }

    #endregion

    #region Tests: Search

    public class SearchTests : LibraryTest
    {
        [Fact]
        public void Matches_first_or_last_name()
        {
            var result = AuthorsController.Search("mar");
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.LastName == "Marsh");
            Assert.Contains(result, a => a.FirstName == "Marcus");
        }

        [Fact]
        public void Is_case_insensitive()
        {
            Assert.Equal(AuthorsController.Search("voss").Count, AuthorsController.Search("VOSS").Count);
        }

        [Fact]
        public void Throws_on_a_too_short_term()
        {
            Assert.Throws<ValidationException>(() => AuthorsController.Search("a"));
            Assert.Throws<ValidationException>(() => AuthorsController.Search("   "));
        }
    }

    #endregion

    #region Tests: GetPage

    public class GetPageTests : LibraryTest
    {
        [Fact]
        public void Returns_the_requested_slice()
        {
            var page = AuthorsController.GetPage(2, 5);
            Assert.Equal(5, page.Count);
        }

        [Fact]
        public void The_last_page_can_be_short()
        {
            Assert.Single(AuthorsController.GetPage(3, 5));
        }

        [Fact]
        public void Paging_past_the_end_is_empty_not_an_error()
        {
            Assert.Empty(AuthorsController.GetPage(99));
        }

        [Fact]
        public void Throws_on_nonsense_paging()
        {
            Assert.Throws<ValidationException>(() => AuthorsController.GetPage(0));
            Assert.Throws<ValidationException>(() => AuthorsController.GetPage(1, 0));
            Assert.Throws<ValidationException>(() => AuthorsController.GetPage(1, 500));
        }
    }

    #endregion

    #region Tests: GetSorted

    public class GetSortedTests : LibraryTest
    {
        [Fact]
        public void Sorts_by_creation_descending()
        {
            var result = AuthorsController.GetSorted(AuthorSort.Created, true);
            Assert.Equal("Kemp", result[0].LastName);
        }

        [Fact]
        public void Returns_the_whole_table_whatever_the_sort()
        {
            Assert.Equal(11, AuthorsController.GetSorted(AuthorSort.BirthDate).Count);
        }
    }

    #endregion

    #region Tests: GetFiltered

    public class GetFilteredTests : LibraryTest
    {
        [Fact]
        public void No_criteria_returns_everyone()
        {
            Assert.Equal(11, AuthorsController.GetFiltered().Count);
        }

        [Fact]
        public void Combines_every_supplied_criterion()
        {
            var result = AuthorsController.GetFiltered(nationality: "Swedish");
            Assert.Equal(2, result.Count);
            Assert.All(result, a => Assert.Equal("Swedish", a.Nationality));
        }

        [Fact]
        public void Filters_on_birth_date_range()
        {
            var result = AuthorsController.GetFiltered(bornAfter: new DateOnly(1980, 1, 1),
                bornBefore: new DateOnly(1990, 1, 1));
            Assert.All(result,
                a => Assert.InRange(a.BirthDate!.Value, new DateOnly(1980, 1, 1), new DateOnly(1990, 1, 1)));
        }

        [Fact]
        public void Throws_when_the_range_is_inverted()
        {
            Assert.Throws<ValidationException>(() =>
                AuthorsController.GetFiltered(bornAfter: new DateOnly(2000, 1, 1),
                    bornBefore: new DateOnly(1990, 1, 1)));
        }
    }

    #endregion

    #region Tests: GetForBook

    public class GetForBookTests : LibraryTest
    {
        [Fact]
        public void Returns_every_credited_author()
        {
            var result = AuthorsController.GetForBook(LibrarySeed.BookIdOf(6));
            Assert.Equal(3, result.Count);
            Assert.Contains(result, a => a.LastName == "Alderholt");
            Assert.Contains(result, a => a.LastName == "Solberg");
            Assert.Contains(result, a => a.LastName == "Lindqvist");
        }

        [Fact]
        public void Returns_empty_for_an_orphan_book()
        {
            Assert.Empty(AuthorsController.GetForBook(LibrarySeed.BookIdOf(12)));
        }

        [Fact]
        public void Throws_for_an_unknown_book()
        {
            Assert.Throws<KeyNotFoundException>(() => AuthorsController.GetForBook(Guid.NewGuid()));
        }
    }

    #endregion

    #region Tests: GetWithoutBooks

    public class GetWithoutBooksTests : LibraryTest
    {
        [Fact]
        public void Finds_the_author_with_no_books()
        {
            var result = AuthorsController.GetWithoutBooks();
            Assert.Single(result);
            Assert.Equal("Kemp", result[0].LastName);
        }
    }

    #endregion
}