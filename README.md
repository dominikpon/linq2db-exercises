# linq2db TDD Exercises

This is Day 1: **white-box** TDD. The entities, DTOs, and method signatures already exist; the
job is to fill in bodies until the pinned test suite passes. Two exercise sets, same shape:

- **Groceries** — a single, wide entity (`Infa/GroceryItem.cs`), exercised via `GroceriesController`.
- **Library** — a many-to-many relationship (`Infa/Author.cs`, `Infa/Book.cs`, `Infa/AuthorBook.cs`),
  exercised via `AuthorsController`, `BooksController` and `AuthorBooksController`.

Day 2 is a different skill — **black-box** API testing, where nothing exists yet and you design
the entities, DTOs and routes yourself against a fixed external contract. See
[`day2/README.md`](day2/README.md).

Both exercise sets also have black-box API checks (curl + sqlite3, no C#): Day 2's are the
exercise itself; the Library ones in [`library-api/`](library-api/README.md) probe how the finished
API behaves for a web client.

## Running things

```bash
dotnet test API/API.csproj                                   # everything
dotnet test API/API.csproj --filter "FullyQualifiedName~Search"  # one method's test region
dotnet run --project API/API.csproj                           # the API itself, Swagger at /swagger
```

## How an exercise is laid out

Each controller is split into `*.Queries.cs` and `*.Commands.cs`. Every action has three parts,
always in this order:

1. An XML doc comment — the spec. `<summary>` says what it does, `<param>`/`<returns>` say what
   goes in and out, and `<exception>` enumerates every validation rule as a thrown type.
2. The method body — this is what you implement. Some already work (read them for the house
   style); most either throw `NotImplementedException` or are simply empty.
3. A `#region Tests: MethodName` immediately below the method — the behavior pinned down as
   `[Fact]`s. Don't edit these, and don't change the method's signature; make the body satisfy them.

Run the region's tests as you go (`--filter "FullyQualifiedName~MethodName"`); don't wait until a
whole controller is done to find out something's wrong.

## Recipe: a query

1. **Validate the request.** Throw `System.ComponentModel.DataAnnotations.ValidationException` for
   anything the doc comment's `<exception>` tags call out — bad paging, an inverted range, a
   too-short search term — before touching the database.
2. **Select the table(s).** Start from `db.Xxx()`. Entities here carry no navigation properties
   (no `Author.Books`, no `.LoadWith(...)`) — the many-to-many is a plain join table, and every
   query that crosses it does so explicitly, e.g.
   `db.Authors().Where(a => db.AuthorBooks().Any(l => l.BookId == bookId && l.AuthorId == a.Id))`.
   The join happens in SQL, not by materializing one side and matching it in C#.
3. **Filter.** Chain `.Where(...)` onto the queryable — one clause per criterion, and only for
   criteria that were actually supplied. A `GetFiltered`-style method builds this up conditionally;
   an omitted filter should never show up in the generated SQL at all.
4. **Sort.** `.OrderBy(...)` / `.OrderByDescending(...)`, still on the queryable. Sorting a `List`
   after `.ToList()` defeats the point — check the generated SQL if you're not sure which one you
   wrote.
5. **Map to the response DTO and return.** `.Select(XxxResponse.Projection)` before executing
   (`.ToList()`, `.FirstOrDefault()`, ...), so the database only ever sends back the columns the
   response actually needs.

## Recipe: a command

1. **Validate the request.** Same rule as queries — `ValidationException` first, before any
   database access.
2. **Look up what you're acting on.** `db.Xxx().FirstOrDefault(x => x.Id == id) ?? throw new
   KeyNotFoundException(...)` for a single row; a bare `.Where(...)` for something bulk.
3. **Check state-dependent business rules.** `InvalidOperationException` for anything that would
   leave the data inconsistent — deleting an author who's still credited on a book, taking a
   barcode that's already in use, and so on. This is the check that can't be expressed as a
   doc-comment `<exception>` on the request shape alone, because it depends on what's already in
   the table.
4. **Mutate.**
   - Single row, a handful of columns: assign properties on the entity you looked up, then
     `db.Update(existing)`.
   - Bulk: `.Set(x => x.Column, x => ...).Update()` — let the database do the arithmetic, don't
     `foreach` and update one row at a time.
   - New row: construct the entity yourself, filling in every server-owned column (`Id`,
     `CreatedAtUtc`, ...) rather than trusting the request for them, then `db.Insert(...)`.
5. **Return.** A response DTO (`new XxxResponse(entity)`) for anything that gives the caller
   something back, or `void` where the doc comment says so. Never the raw linq2db entity.

## Update requests

A single update endpoint per entity takes a request where every property except the id is
optional, so a caller can send only the columns they're changing. A `null` (or omitted) property
means "leave the column alone"; anything else is validated and applied. Consequently, a nullable
column can't be set back to `null` through the update endpoint.

## DTOs

Request/response DTOs (`API/Dtos/`) are generated from the entities with the
[`Facet`](https://github.com/Tim-Maes/Facet) source generator rather than written by hand — see
the `[Facet(...)]` attribute on each `partial record`. `exclude` keeps server-owned columns
(`Id`, `CreatedAtUtc`, ...) off create requests; update requests add `NullableProperties = true`
so every generated property becomes optional, plus a hand-declared `Id`.
