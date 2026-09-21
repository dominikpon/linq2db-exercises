# linq2db TDD Exercises

## How an exercise is laid out

Each controller is split into `*.Queries.cs` and `*.Commands.cs`. Every action has three parts,
always in this order:

1. The commant above the controller method = the spec(ification). `<summary>` says what it does, `<param>`/`<returns>` say what
   goes in and out, and `<exception>` mentions every validation rule.
2. The method body: This is what you implement. Every one starts as
   `throw new NotImplementedException();`, so the whole suite begins red. Helpers (validation,
   uniqueness checks) are yours to write too.
3. A `#region Tests: MethodName` immediately below the method: The behavior pinned down as
   `[Fact]`s. Don't edit these, and don't change the method's signature; make the body pass the test.

## Recipe: a query

1. **Validate the request.** Throw `System.ComponentModel.DataAnnotations.ValidationException` for
   anything the doc comment's `<exception>` tags call out — bad paging, an inverted range, a
   too-short search term — before touching the database.
2. **Select the table(s).** Start from `db.Xxx()`. Flat queries stay explicit: the many-to-many is a
   plain join table, so a list of books for one author is
   `db.Books().Where(b => db.AuthorBooks().Any(l => l.AuthorId == authorId && l.BookId == b.Id))`,
   joined in SQL rather than matched in C#. When the response is **nested** (a book with its
   authors, an author with their books) use the association properties `Book.Authors` and
   `Author.Books` and ask for them with `.LoadWith(b => b.Authors)`. They are not columns and stay
   empty until you load them; see `LibraryQueriesController`.
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
2. **Look up what you're acting on.** `db.Xxx().FirstOrDefault(x => x.Id == id) ?? throw new KeyNotFoundException(...)` for a single row; a bare `.Where(...)` for something bulk.
3. **Check state-dependent business rules.** `InvalidOperationException` for anything that would
   leave the data inconsistent — deleting an author who's still credited on a book, taking a
   barcode that's already in use, and so on. This is the check that can't depend on the request alone, because it depends on what's already in the table.
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

Each entity has two ways to change an existing row, both idempotent:

- **`PUT .../Replace`** replaces the whole mutable row. The request carries every mutable column
  (the entity minus `CreatedAtUtc`), so a `null` on a nullable column really clears it. A client's
  edit form GETs the object, changes it and PUTs the whole thing back.
- **`PATCH .../Update`** changes only what was sent. Every property except the id is optional, and
  a `null` (or omitted) property means "leave the column alone". It can never clear a nullable
  column; that is what `Replace` is for.

Both apply the same validation rules as `Create`, to the values that are actually being written.

## DTOs

Request/response DTOs (`API/Dtos/`) are generated from the entities with the
[`Facet`](https://github.com/Tim-Maes/Facet) source generator rather than written by hand — see
the `[Facet(...)]` attribute on each `partial record`. `exclude` keeps server-owned columns
(`Id`, `CreatedAtUtc`, ...) off create requests; replace requests keep the `Id` and drop only
`CreatedAtUtc`; patch requests add `NullableProperties = true` so every generated property becomes
optional, plus a hand-declared `Id`.

Nested responses are plain records that inherit the flat response and add one list:
`BookWithAuthorsResponse : BookResponse` (adds `Authors`) and `AuthorWithBooksResponse : AuthorResponse`
(adds `Books`). Their constructor takes the entity, so load the association with `LoadWith` first and
then `new BookWithAuthorsResponse(book)`. The nested items are the flat responses, which never carry
the association back, so there are no cycles. Every flat DTO lists the association property in its
`exclude`. The order of the nested list is not specified.
