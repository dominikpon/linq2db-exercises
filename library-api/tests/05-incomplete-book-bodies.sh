# JSON can't say "missing" for a number or enum once it lands in a non-nullable C# type.
req POST /Books/Create '{"title":"NoGenreNoPrice"}'
expect "a book without genre and price is not silently created with defaults" "$(sql "SELECT COUNT(*) FROM Books WHERE Title='NoGenreNoPrice'")" "0"

mid=$(sql "SELECT COUNT(*) FROM Books")
req POST /Books/Create '{"genre":0,"priceDkk":10}'
expect "a book without a title is not created" "$(sql "SELECT COUNT(*) FROM Books")" "$mid"

req POST /Books/Create '{"title":"NullPrice","genre":0,"priceDkk":null}'
expect "an explicit null price is not created as 0" "$(sql "SELECT COUNT(*) FROM Books WHERE Title='NullPrice'")" "0"
