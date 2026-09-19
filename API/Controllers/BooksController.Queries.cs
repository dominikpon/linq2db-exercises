using System.ComponentModel.DataAnnotations;
using API.Dtos;
using API.Enums;
using Infa;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace API.Controllers;

[Route("[controller]")]
public partial class BooksController(LibraryDatabase db) : ControllerBase
{
    /// <summary>Every book, ordered by title.</summary>
    /// <returns>All 13 seeded books.</returns>
    [HttpGet(nameof(GetAll))]
    public List<BookResponse> GetAll()
    {
        return db.Books()
            .OrderBy(b => b.Title)
            .Select(BookResponse.Projection)
            .ToList();
    }

    /// <summary>One book looked up by primary key.</summary>
    /// <exception cref="KeyNotFoundException">No row has that id.</exception>
    [HttpGet(nameof(GetById))]
    public BookResponse GetById([FromQuery] Guid id)
    {
        return db.Books()
            .Where(b => b.Id == id)
            .Select(BookResponse.Projection)
            .FirstOrDefault() ?? throw new KeyNotFoundException("that book does not exist");
    }

    /// <summary>How many books the table holds.</summary>
    [HttpGet(nameof(Count))]
    public int Count()
    {
        return db.Books().Count();
    }

    /// <summary>Free-text search over <see cref="Book.Title" />. A partial, case-insensitive match is enough.</summary>
    /// <param name="q">At least two characters, otherwise the search is meaningless.</param>
    /// <exception cref="ValidationException"><paramref name="q" /> is null, blank or shorter than two characters.</exception>
    [HttpGet(nameof(Search))]
    public List<BookResponse> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            throw new ValidationException("search term must be at least two characters");

        var term = q.Trim();
        return db.Books()
            .Where(b => b.Title.Contains(term))
            .OrderBy(b => b.Title)
            .Select(BookResponse.Projection)
            .ToList();
    }

    /// <summary>One page of books, ordered by title.</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="size">Rows per page, at most 100.</param>
    /// <exception cref="ValidationException">
    ///     <paramref name="page" /> is below 1, or <paramref name="size" /> is outside
    ///     1..100.
    /// </exception>
    [HttpGet(nameof(GetPage))]
    public List<BookResponse> GetPage([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        if (page < 1)
            throw new ValidationException("page must be at least 1");
        if (size < 1 || size > 100)
            throw new ValidationException("size must be between 1 and 100");

        return db.Books()
            .OrderBy(b => b.Title)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(BookResponse.Projection)
            .ToList();
    }

    /// <summary>Every book, sorted by a caller-chosen column. Sorting happens in SQL.</summary>
    [HttpGet(nameof(GetSorted))]
    public List<BookResponse> GetSorted([FromQuery] BookSort by, [FromQuery] bool descending = false)
    {
        var q = by switch
        {
            BookSort.Title => descending
                ? db.Books().OrderByDescending(b => b.Title)
                : db.Books().OrderBy(b => b.Title),
            BookSort.Price => descending
                ? db.Books().OrderByDescending(b => b.PriceDkk)
                : db.Books().OrderBy(b => b.PriceDkk),
            BookSort.Published => descending
                ? db.Books().OrderByDescending(b => b.PublishedDate)
                : db.Books().OrderBy(b => b.PublishedDate),
            BookSort.Created => descending
                ? db.Books().OrderByDescending(b => b.CreatedAtUtc)
                : db.Books().OrderBy(b => b.CreatedAtUtc),
            _ => throw new ArgumentOutOfRangeException(nameof(by))
        };

        return q.Select(BookResponse.Projection).ToList();
    }

    /// <summary>
    ///     A filtered list. Every parameter that is supplied narrows the result; every parameter left
    ///     null is ignored.
    /// </summary>
    /// <param name="q">Matches the title, case-insensitive and partial.</param>
    /// <param name="genre">Exact match.</param>
    /// <param name="outOfPrint">Only out-of-print books, or only in-print books.</param>
    /// <param name="minPrice">Inclusive lower price bound.</param>
    /// <param name="maxPrice">Inclusive upper price bound.</param>
    /// <returns>Matching books, ordered by title. No parameters at all returns everything.</returns>
    /// <exception cref="ValidationException"><paramref name="minPrice" /> is greater than <paramref name="maxPrice" />.</exception>
    [HttpGet(nameof(GetFiltered))]
    public List<BookResponse> GetFiltered(
        [FromQuery] string? q = null,
        [FromQuery] Genre? genre = null,
        [FromQuery] bool? outOfPrint = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        if (minPrice != null && maxPrice != null && minPrice > maxPrice)
            throw new ValidationException("minPrice cannot be greater than maxPrice");

        var query = db.Books().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(b => b.Title.Contains(q));
        if (genre != null)
            query = query.Where(b => b.Genre == genre);
        if (outOfPrint != null)
            query = query.Where(b => b.IsOutOfPrint == outOfPrint);
        if (minPrice != null)
            query = query.Where(b => b.PriceDkk >= minPrice);
        if (maxPrice != null)
            query = query.Where(b => b.PriceDkk <= maxPrice);

        return query
            .OrderBy(b => b.Title)
            .Select(BookResponse.Projection)
            .ToList();
    }

    /// <summary>Every book credited to one author, ordered by title.</summary>
    /// <exception cref="KeyNotFoundException">No author has that id.</exception>
    [HttpGet(nameof(GetByAuthor))]
    public List<BookResponse> GetByAuthor([FromQuery] Guid authorId)
    {
        if (!db.Authors().Any(a => a.Id == authorId))
            throw new KeyNotFoundException("that author does not exist");

        return db.Books()
            .Where(b => db.AuthorBooks().Any(l => l.AuthorId == authorId && l.BookId == b.Id))
            .OrderBy(b => b.Title)
            .Select(BookResponse.Projection)
            .ToList();
    }

    /// <summary>Books with no author credited at all.</summary>
    /// <returns>1 book on the seed data.</returns>
    [HttpGet(nameof(GetWithoutAuthors))]
    public List<BookResponse> GetWithoutAuthors()
    {
        return db.Books()
            .Where(b => !db.AuthorBooks().Any(l => l.BookId == b.Id))
            .OrderBy(b => b.Title)
            .Select(BookResponse.Projection)
            .ToList();
    }

    /// <summary>The average price across the catalogue, as a scalar.</summary>
    [HttpGet(nameof(GetAveragePrice))]
    public decimal GetAveragePrice()
    {
        return db.Books().Average(b => b.PriceDkk);
    }

    #region Tests: GetAll

    public class GetAllTests : LibraryTest
    {
        [Fact]
        public void Returns_every_seeded_book()
        {
            Assert.Equal(13, BooksController.GetAll().Count);
        }

        [Fact]
        public void Is_ordered_by_title()
        {
            var titles = BooksController.GetAll().Select(b => b.Title).ToList();
            Assert.Equal(titles.OrderBy(t => t, StringComparer.Ordinal), titles);
        }
    }

    #endregion

    #region Tests: GetById

    public class GetByIdTests : LibraryTest
    {
        [Fact]
        public void Returns_the_book_for_a_known_id()
        {
            var book = BooksController.GetById(LibrarySeed.BookIdOf(1));
            Assert.Equal("The Glass Meridian", book.Title);
        }

        [Fact]
        public void Throws_when_the_id_is_unknown()
        {
            Assert.Throws<KeyNotFoundException>(() => BooksController.GetById(Guid.NewGuid()));
        }
    }

    #endregion

    #region Tests: Count

    public class CountTests : LibraryTest
    {
        [Fact]
        public void Counts_the_seeded_rows()
        {
            Assert.Equal(13, BooksController.Count());
        }
    }

    #endregion

    #region Tests: Search

    public class SearchTests : LibraryTest
    {
        [Fact]
        public void Matches_the_title()
        {
            var result = BooksController.Search("wake");
            Assert.Single(result);
            Assert.Equal("Northern Wake", result[0].Title);
        }

        [Fact]
        public void Is_case_insensitive()
        {
            Assert.Equal(BooksController.Search("wake").Count, BooksController.Search("WAKE").Count);
        }

        [Fact]
        public void Throws_on_a_too_short_term()
        {
            Assert.Throws<ValidationException>(() => BooksController.Search("a"));
        }
    }

    #endregion

    #region Tests: GetPage

    public class GetPageTests : LibraryTest
    {
        [Fact]
        public void Returns_the_requested_slice()
        {
            Assert.Equal(5, BooksController.GetPage(2, 5).Count);
        }

        [Fact]
        public void The_last_page_can_be_short()
        {
            Assert.Equal(3, BooksController.GetPage(3, 5).Count);
        }

        [Fact]
        public void Throws_on_nonsense_paging()
        {
            Assert.Throws<ValidationException>(() => BooksController.GetPage(0));
            Assert.Throws<ValidationException>(() => BooksController.GetPage(1, 0));
        }
    }

    #endregion

    #region Tests: GetSorted

    public class GetSortedTests : LibraryTest
    {
        [Fact]
        public void Sorts_by_price_ascending()
        {
            var prices = BooksController.GetSorted(BookSort.Price).Select(b => b.PriceDkk).ToList();
            Assert.Equal(prices.OrderBy(p => p), prices);
        }

        [Fact]
        public void Sorts_by_creation_descending()
        {
            var result = BooksController.GetSorted(BookSort.Created, true);
            Assert.Equal("The Last Ledger", result[0].Title);
        }

        [Fact]
        public void Returns_the_whole_table_whatever_the_sort()
        {
            Assert.Equal(13, BooksController.GetSorted(BookSort.Title).Count);
        }
    }

    #endregion

    #region Tests: GetFiltered

    public class GetFilteredTests : LibraryTest
    {
        [Fact]
        public void No_criteria_returns_everything()
        {
            Assert.Equal(13, BooksController.GetFiltered().Count);
        }

        [Fact]
        public void Combines_every_supplied_criterion()
        {
            var result = BooksController.GetFiltered(genre: Genre.Fantasy, outOfPrint: true);
            Assert.Single(result);
            Assert.Equal("Cinder and Salt", result[0].Title);
        }

        [Fact]
        public void Filters_on_price_range()
        {
            var result = BooksController.GetFiltered(minPrice: 200m, maxPrice: 230m);
            Assert.Equal(2, result.Count);
            Assert.All(result, b => Assert.True(b.PriceDkk >= 200m && b.PriceDkk <= 230m));
        }

        [Fact]
        public void Throws_when_the_range_is_inverted()
        {
            Assert.Throws<ValidationException>(() => BooksController.GetFiltered(minPrice: 50m, maxPrice: 10m));
        }
    }

    #endregion

    #region Tests: GetByAuthor

    public class GetByAuthorTests : LibraryTest
    {
        [Fact]
        public void Returns_every_book_credited_to_the_author()
        {
            var result = BooksController.GetByAuthor(LibrarySeed.AuthorIdOf(4));
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void Returns_empty_for_an_author_with_no_books()
        {
            Assert.Empty(BooksController.GetByAuthor(LibrarySeed.AuthorIdOf(11)));
        }

        [Fact]
        public void Throws_for_an_unknown_author()
        {
            Assert.Throws<KeyNotFoundException>(() => BooksController.GetByAuthor(Guid.NewGuid()));
        }
    }

    #endregion

    #region Tests: GetWithoutAuthors

    public class GetWithoutAuthorsTests : LibraryTest
    {
        [Fact]
        public void Finds_the_orphan_book()
        {
            var result = BooksController.GetWithoutAuthors();
            Assert.Single(result);
            Assert.Equal("Unwritten Kingdoms", result[0].Title);
        }
    }

    #endregion

    #region Tests: GetAveragePrice

    public class GetAveragePriceTests : LibraryTest
    {
        [Fact]
        public void Averages_every_row()
        {
            Assert.Equal(179.81m, Math.Round(BooksController.GetAveragePrice(), 2));
        }
    }

    #endregion
}