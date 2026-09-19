req POST /Books/Create '{"title":"E1","genre":"SciFi","priceDkk":10}'
expect "genre echoed as the enum name" "$(jqb .genre)" "SciFi"
expect "db stores the mapped value, not the name" "$(sql "SELECT Genre FROM Books WHERE Title='E1'")" "sci_fi"

req POST /Books/Create '{"title":"E2","genre":"scifi","priceDkk":10}'
expect "lowercase genre is accepted" "$(sql "SELECT COUNT(*) FROM Books WHERE Title='E2'")" "1"

want=$(sql "SELECT COUNT(*) FROM Books WHERE Genre='fantasy'")
qget /Books/GetFiltered genre=Fantasy
expect "filter by genre name" "$(jq length <<<"$BODY")" "$want"
qget /Books/GetFiltered genre=fantasy
expect "filter by lowercase genre name" "$(jq length <<<"$BODY")" "$want"
