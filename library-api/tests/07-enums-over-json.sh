# Default System.Text.Json: an enum travels as its number (Genre: Fiction=0 NonFiction=1 SciFi=2 Fantasy=3 ...).
req POST /Books/Create '{"title":"E1","genre":2,"priceDkk":10}'
expect "genre echoed as the enum number" "$(jqb .genre)" "2"
expect "db stores the mapped value, not the number" "$(sql "SELECT Genre FROM Books WHERE Title='E1'")" "sci_fi"

req POST /Books/Create '{"title":"E2","genre":"SciFi","priceDkk":10}'
info "genre sent as a string name, db rows created" "$(sql "SELECT COUNT(*) FROM Books WHERE Title='E2'")"

want=$(sql "SELECT COUNT(*) FROM Books WHERE Genre='fantasy'")
qget /Books/GetFiltered genre=3
expect "query string filter by genre number" "$(jq length <<<"$BODY")" "$want"
qget /Books/GetFiltered genre=Fantasy
expect "query string filter by genre name" "$(jq length <<<"$BODY")" "$want"
