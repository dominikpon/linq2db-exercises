# Library API — web-client checks

Black-box tests for the finished Library API, written from the point of view of a browser/TypeScript
client rather than of the C# code: JSON in, JSON out over `curl`, and `sqlite3` looking straight at
the database file to see what actually got stored. They exist to discover where the JSON <-> C#
boundary bites (dates without a `Z`, missing numbers becoming `0`, unicode case-folding,
ISO datetimes in query strings, ...), not to pin status codes — codes are printed for context and
never asserted.

```bash
./library-api/run.sh              # every test file
./library-api/run.sh 07 12        # only files starting with these prefixes
```

Needs `curl`, `jq` and `sqlite3`. Each file in `tests/` gets a fresh database and a freshly started
server (the app seeds the library itself), so files never affect each other. A file is a plain bash
script using the helpers in `lib.sh`:

| Helper | What it does |
|---|---|
| `req METHOD PATH [json]` | send a request, set `STATUS` and `BODY` |
| `qget PATH k=v ...` | GET with url-encoded query parameters |
| `jqb '.field'` | pull a value out of `BODY` |
| `sql "SELECT ..."` | raw SQLite lookup |
| `expect` / `expect_not` / `expect_match` | a check; a failing check is reported, the file carries on |
| `info label value` | print something worth knowing that isn't pass/fail |
| `aid N` / `bid N` | the seeded author / book id as a string |

`Guid` columns are byte-swapped blobs in SQLite, so look rows up by a readable column
(`WHERE LastName='Marsh'`), not by id.

A `FAIL` means "a web client would be surprised by this", not necessarily "the code is wrong".
