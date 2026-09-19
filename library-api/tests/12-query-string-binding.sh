qget /Authors/GetPage page=2 size=5
expect "page 2 of authors" "$(jq length <<<"$BODY")" "5"
qget /Authors/GetPage page=3 size=5
expect "short last page" "$(jq length <<<"$BODY")" "1"

max=$(sql "SELECT MAX(PriceDkk) FROM Books")
qget /Books/GetSorted by=Price descending=true
expect "sorted by price descending, exact enum name" "$(jqb '.[0].priceDkk')" "$max"
qget /Books/GetSorted by=price descending=True
expect "sorted by lowercase enum name and capitalised bool" "$(jqb '.[0].priceDkk')" "$max"

qget /Books/GetFiltered minPrice=200 maxPrice=230
expect "price range" "$(jq length <<<"$BODY")" "$(sql "SELECT COUNT(*) FROM Books WHERE PriceDkk>=200 AND PriceDkk<=230")"
qget /Books/GetFiltered outOfPrint=true
expect "boolean filter" "$(jq length <<<"$BODY")" "$(sql "SELECT COUNT(*) FROM Books WHERE IsOutOfPrint=1")"

want=$(sql "SELECT COUNT(*) FROM Authors WHERE BirthDate>='1980-01-01' AND BirthDate<='1990-01-01'")
qget /Authors/GetFiltered bornAfter=1980-01-01 bornBefore=1990-01-01
expect "date range as plain dates" "$(jq length <<<"$BODY")" "$want"
qget /Authors/GetFiltered bornAfter=1980-01-01T00:00:00Z bornBefore=1990-01-01T00:00:00Z
expect "date range as JS toISOString() values" "$(jq length <<<"$BODY")" "$want"
