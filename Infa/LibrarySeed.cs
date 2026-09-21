using LinqToDB;
using LinqToDB.Data;

namespace Infa;

public static class LibrarySeed
{
    public static void EnsureSeeded(LibraryDatabase db)
    {
        db.CreateTable<Author>(tableOptions: TableOptions.CreateIfNotExists);
        db.CreateTable<Book>(tableOptions: TableOptions.CreateIfNotExists);
        db.CreateTable<AuthorBook>(tableOptions: TableOptions.CreateIfNotExists);
        if (db.Authors().Any()) return;
        db.BulkCopy(BuildAuthors());
        db.BulkCopy(BuildBooks());
        db.BulkCopy(BuildLinks());
    }

    public static string AuthorIdOf(int index)
    {
        return index.ToString();
    }

    public static string BookIdOf(int index)
    {
        return index.ToString();
    }

    public static Author[] BuildAuthors()
    {
        var today = DateTime.UtcNow.Date;
        var id = 0;

        Author Person(string first, string last, string? nationality, string? bio, string? website,
            DateOnly? birthDate, int createdDaysAgo)
        {
            return new Author
            {
                Id = AuthorIdOf(++id),
                FirstName = first,
                LastName = last,
                Nationality = nationality,
                Bio = bio,
                Website = website,
                BirthDate = birthDate,
                CreatedAtUtc = today.AddDays(-createdDaysAgo).AddHours(9)
            };
        }

        return
        [
            Person("Elena", "Marsh", "British", "Literary novelist known for slow-burn family sagas.",
                "https://elenamarsh.example.com", new DateOnly(1978, 4, 12), 900),
            Person("Tobias", "Reyne", "German", null, null, new DateOnly(1985, 9, 3), 850),
            Person("Priya", "Chandrasekaran", "Indian", "Tech journalist turned author.",
                "https://priyawrites.example.com", new DateOnly(1990, 1, 20), 800),
            Person("Magnus", "Alderholt", "Swedish", "Prolific mystery writer with a cult following.", null,
                new DateOnly(1965, 11, 30), 750),
            Person("Naomi", "Okafor", "Nigerian", null, "https://naomiokafor.example.com", new DateOnly(1982, 6, 15),
                700),
            Person("Julian", "Voss", "German", "Reclusive sci-fi author.", null, null, 650),
            Person("Ingrid", "Solberg", "Norwegian", null, null, new DateOnly(1993, 3, 8), 600),
            Person("Marcus", "Whitfield", "British", "Fantasy world-builder.", "https://marcuswhitfield.example.com",
                new DateOnly(1971, 7, 22), 500),
            Person("Sofia", "Lindqvist", "Swedish", null, null, new DateOnly(1988, 12, 5), 400),
            Person("Devon", "Blackwood", "American", "Biographer and historian.", "https://devonblackwood.example.com",
                new DateOnly(1960, 2, 14), 200),
            Person("Rosalind", "Kemp", "British", "Debut author awaiting her first release.", null,
                new DateOnly(1995, 5, 19), 10)
        ];
    }

    public static Book[] BuildBooks()
    {
        var today = DateTime.UtcNow.Date;
        var id = 0;

        Book Publication(string title, string? isbn, Genre genre, decimal price, bool outOfPrint,
            DateOnly? published, int createdDaysAgo)
        {
            return new Book
            {
                Id = BookIdOf(++id),
                Title = title,
                Isbn = isbn,
                Genre = genre,
                PriceDkk = price,
                IsOutOfPrint = outOfPrint,
                PublishedDate = published,
                CreatedAtUtc = today.AddDays(-createdDaysAgo).AddHours(9)
            };
        }

        return
        [
            Publication("The Glass Meridian", "9788700000011", Genre.Fiction, 149.00m, false, new DateOnly(2015, 4, 10),
                850),
            Publication("Echoes of Tomorrow", "9788700000028", Genre.SciFi, 179.50m, false, new DateOnly(2019, 8, 22),
                800),
            Publication("Cinder and Salt", null, Genre.Fantasy, 159.00m, true, new DateOnly(2010, 1, 15), 780),
            Publication("The Silent Algorithm", "9788700000042", Genre.NonFiction, 219.00m, false,
                new DateOnly(2022, 3, 1), 700),
            Publication("Northern Wake", "9788700000059", Genre.Mystery, 139.00m, false, new DateOnly(2017, 11, 11),
                650),
            Publication("Paper Moons", null, Genre.Fiction, 169.00m, false, null, 600),
            Publication("The Long Wager", "9788700000066", Genre.Mystery, 149.00m, true, new DateOnly(2012, 6, 6), 580),
            Publication("A History of Rain", "9788700000073", Genre.NonFiction, 249.00m, false,
                new DateOnly(2021, 9, 9), 500),
            Publication("The Cartographer's Daughter", null, Genre.Fiction, 179.00m, false, new DateOnly(2023, 2, 14),
                400),
            Publication("Static Bloom", "9788700000080", Genre.SciFi, 189.00m, false, new DateOnly(2020, 5, 5), 350),
            Publication("Wolves at the Border", "9788700000097", Genre.Fantasy, 199.00m, false,
                new DateOnly(2018, 10, 30), 300),
            Publication("Unwritten Kingdoms", null, Genre.Fantasy, 129.00m, false, null, 200),
            Publication("The Last Ledger", "9788700000103", Genre.Biography, 229.00m, false, new DateOnly(2024, 1, 20),
                30)
        ];
    }

    /// <summary>
    ///     Book 12 ("Unwritten Kingdoms") is deliberately left without an author, and author 11
    ///     ("Rosalind Kemp") is deliberately left without a book — both exist to exercise the
    ///     "nothing linked" edge of the many-to-many queries.
    /// </summary>
    public static AuthorBook[] BuildLinks()
    {
        AuthorBook Link(int author, int book)
        {
            return new AuthorBook { AuthorId = AuthorIdOf(author), BookId = BookIdOf(book) };
        }

        return
        [
            Link(1, 1), Link(1, 2), // Elena Marsh
            Link(2, 2), Link(2, 3), // Tobias Reyne
            Link(3, 4), // Priya Chandrasekaran
            Link(4, 5), Link(4, 6), Link(4, 7), // Magnus Alderholt
            Link(5, 8), Link(5, 9), // Naomi Okafor
            Link(6, 10), Link(6, 11), // Julian Voss
            Link(7, 6), // Ingrid Solberg
            Link(8, 11), // Marcus Whitfield
            Link(9, 6), // Sofia Lindqvist
            Link(10, 13) // Devon Blackwood
        ];
    }
}