# Day 2 — Black-Box API Testing

Day 1 (`Infa/Author.cs`, `Infa/Book.cs`, the `AuthorsController`/`BooksController` split, and the
Grocery exercise before it) is **white-box**: the entities, DTOs, and method signatures already
exist, and the job is to fill in bodies until the tests pinned to each method pass.

Day 2 flips that. Nothing exists yet — no entity, no DTO, no controller, no route. The only thing
that's fixed is an external contract: table names, column names, HTTP routes, and JSON shapes.
The tests never touch a line of your C# — they start the real API process, seed SQLite directly
with `sqlite3`, fire an HTTP request with `curl`, and check the response. You decide everything
about how the inside is built; the tests only ever see the outside.

This is a genuinely different skill from Day 1: instead of "make this signature's body do what
the doc comment says," it's "design a signature, a DTO, a mapping, and a route so that a fixed,
already-written external contract is satisfied." Both matter — most real backend work is closer
to Day 2 than Day 1.

## The domain: a task tracker

A new, unrelated domain area — `Project`, `Task`, `Tag`, linked many-to-many through a `TaskTag`
junction, structurally the same shape as Day 1's `Author`/`Book`/`AuthorBook` on purpose, so the
black-box skill is what's new, not the modeling problem.

- A **Project** has many **Tasks**.
- A **Task** belongs to one Project and can carry many **Tags**; a **Tag** can be on many Tasks
  (`TaskTag` is the bare junction, exactly like `AuthorBook`).

> **Naming gotcha:** don't call your entity class `Task` — it collides with
> `System.Threading.Tasks.Task`, which is in scope everywhere via this project's implicit usings.
> Call it `TaskItem` (or fully qualify every use of the real `Task`). The **table** is still named
> `Tasks` regardless of what you call the C# class — see the schema below.

## The fixed contract

Everything in this section is not negotiable — the seed scripts and the `.http` requests are
already written against it. Everything *outside* this section (the entity classes themselves, the
DTOs, the controller code, your validation, how you wire up the `DataConnection`) is yours to
design.

### Schema

linq2db needs `[Table]`/`[Column]` names that match these exactly (property name = column name,
same as `Author`/`Book`), because the seed scripts insert rows with raw SQL, bypassing your C#
entirely:

```
Projects   Id (string, PK), Name (string), Description (string?), CreatedAtUtc (DateTime)
Tasks      Id (string, PK), ProjectId (string), Title (string), Status (string), DueDate (DateOnly?), CreatedAtUtc (DateTime)
Tags       Id (string, PK), Name (string)
TaskTags   TaskId (string, PK part 1), TagId (string, PK part 2)   — bare junction, like AuthorBook
```

Every id here is a **`string`**, not a `Guid` — generate it yourself with `Guid.NewGuid().ToString()`
when you create a row. That's deliberate: a `string` column holds exactly the text you'd expect
(`"10000000-0000-4000-a000-000000000001"`), so `seed.sql` can write a plain quoted literal and it
just matches. (Day 1's `Guid`-typed ids are stored by linq2db's SQLite provider as a byte-swapped
blob, not as text — fine for Day 1, where nothing seeds rows by hand, but exactly the kind of
storage detail a black-box test shouldn't need to know about.)

`Status` must round-trip as the literal strings `"Todo"`, `"InProgress"`, `"Done"` — both in the
database and in JSON. Use `[MapValue("Todo")]` etc. on your enum, exactly the way `Genre` and
`StorageType` already do elsewhere in this repo. A plain enum with no `MapValue` stores as an
`int`, which the seed scripts do not know how to write.

### Routes and JSON

Same bare-route convention as Day 1 (`[Route("[controller]")]`, no `/api` prefix). Field names in
JSON are camelCase (the default — same as every Day 1 response you've already seen).

| Method | Route | Body | Success | Notes |
|---|---|---|---|---|
| POST | `/Projects/Create` | `{ name, description? }` | 200, Project | `name` required |
| GET | `/Projects/GetById?id=` | — | 200, Project / **404** | |
| GET | `/Projects/GetAll` | — | 200, Project[] | |
| POST | `/Tasks/Create` | `{ projectId, title, status?, dueDate? }` | 200, Task | `status` omitted → `"Todo"`; **404** if `projectId` doesn't exist |
| GET | `/Tasks/GetById?id=` | — | 200, Task / **404** | |
| PUT | `/Tasks/Update` | `{ id, title?, status?, dueDate? }` | 200, Task | omitted/null → leave alone (same as Day 1: there's no way to null out `dueDate` here) |
| DELETE | `/Tasks/Delete?id=` | — | 200, empty body | |
| GET | `/Tasks/GetByProject?projectId=` | — | 200, Task[] / **404** if project unknown | |
| GET | `/Tasks/GetByTag?tagId=` | — | 200, Task[] / **404** if tag unknown | |
| POST | `/Tags/Create` | `{ name }` | 200, Tag | **409** if `name` already taken |
| GET | `/Tags/GetAll` | — | 200, Tag[] | |
| GET | `/Tags/GetByTask?taskId=` | — | 200, Tag[] / **404** if task unknown | |
| POST | `/TaskTags/Link?taskId=&tagId=` | — | 200, empty body | idempotent; **404** if either id is unknown |
| DELETE | `/TaskTags/Unlink?taskId=&tagId=` | — | 200, empty body | idempotent, even if never linked |

`Project` JSON: `{ id, name, description, createdAtUtc }`. `Task` JSON: `{ id, projectId, title,
status, dueDate, createdAtUtc }`. `Tag` JSON: `{ id, name }`.

### Status codes

Thrown from your controller, the same three built-in exceptions Day 1 uses, mapped by the shared
handler in `API/Program.cs`:

```
ValidationException (System.ComponentModel.DataAnnotations)  -> 400
KeyNotFoundException (System.Collections.Generic)             -> 404
InvalidOperationException (System)                             -> 409
```

## Recipe

Same shape as Day 1's, with one step added at the front:

1. **Design the entity and the table mapping.** `[Table]`/`[Column]`/`[PrimaryKey]` matching the
   schema above exactly — this is the step that didn't exist on Day 1.
2. **Design the request/response DTOs.** Match the JSON shapes above. Nothing says you have to
   use `Facet` here the way Day 1 does — plain records are fine — but you can if you want the
   practice.
3. **Wire up the route and the DI registration** — a `DataConnection` subclass and its
   registration in `Program.cs`, the same pattern as `LibraryDatabase`.
4. **Validate, look up, mutate, return** — same recipe as the root `README.md`'s "Recipe: a
   query" / "Recipe: a command", just applied to a contract you designed the DTOs for instead of
   one that was handed to you.
5. **Run the test.** `./day2/run-test.sh day2/tests/<name>` for one, `./day2/run-all.sh` for all
   of them. Read the failure message — it prints the actual response body next to what failed.

## How the tests actually work

Each folder under `tests/` is one black-box test:

```
tests/tasks-update-partial/
  seed.sql       # raw SQL, run directly against a fresh, empty SQLite file with `sqlite3`
  request.http   # the HTTP request — first line "METHOD url", optional JSON body after a blank line
  expect.json    # { "status": 200, "jq": "<boolean jq expression, or null to skip body checks>" }
```

`run-test.sh` does exactly this, per test, from scratch every time:

1. Create a brand-new SQLite file.
2. Start the real API (`dotnet run --project API/API.csproj`) pointed at it via the same `DB`
   environment variable `Program.cs` already reads. Your startup code still creates the tables
   (`CreateTable<T>`, same as `GrocerySeed`/`LibrarySeed`) — only the *rows* are seeded outside C#.
3. Wait for the server to answer, then apply `seed.sql` straight to the SQLite file with the
   `sqlite3` CLI — no C#, no HTTP, involved at all.
4. Fire the one request in `request.http` with `curl`.
5. Check the status code and, unless `expect.json`'s `"jq"` is `null`, run that expression against
   the JSON response body with `jq -e`.
6. Tear the server down and delete the database file.

That's also why each test is independent: a delete test doesn't need a create test to have run
first, because its `seed.sql` inserts the row it's about to delete directly.

## Don't read the tests until one fails

Work from the contract above, not from the fixtures. Opening `tests/*/seed.sql` or
`request.http` before you've implemented something defeats the point of a black-box test — it
turns "does my API satisfy the spec" into "does my API match this one worked example," which is
a much easier and much less useful thing to satisfy.

The rule: don't open anything under `tests/` while you're building. Implement from the contract
and the routes table alone. Only open a specific test's three files — `seed.sql`, `request.http`,
`expect.json` — *after* running it and seeing `FAIL`, to see exactly what was seeded, what was
sent, and what was expected, so you can work out why. `run-test.sh`'s failure output already gives
you the actual response body; the test's own files are the next thing to check, not the first.

## Running everything

```bash
./day2/run-all.sh                        # every test, with a pass/fail summary
./day2/run-test.sh day2/tests/tasks-create  # just one, while you're working on it
```
