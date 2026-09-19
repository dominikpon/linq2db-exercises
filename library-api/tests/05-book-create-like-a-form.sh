# Payloads shaped by the generated type: title, genre and priceDkk are required; isbn and publishedDate are `T | null`.
req POST /Books/Create '{"title":"Minimal","genre":0,"priceDkk":10}'
expect "only the required fields creates a book" "$(sql "SELECT COUNT(*) FROM Books WHERE Title='Minimal'")" "1"

req POST /Books/Create '{"title":"WithNulls","isbn":null,"genre":1,"priceDkk":10,"publishedDate":null}'
expect "explicit nulls for the nullable fields create a book" "$(sql "SELECT COUNT(*) FROM Books WHERE Title='WithNulls'")" "1"
expect "isbn and publishedDate are stored as NULL" "$(sql "SELECT (Isbn IS NULL) || (PublishedDate IS NULL) FROM Books WHERE Title='WithNulls'")" "11"

req POST /Books/Create '{"title":"Dated","genre":0,"priceDkk":10,"publishedDate":"2025-03-01"}'
expect "publishedDate is stored as yyyy-MM-dd" "$(sql "SELECT PublishedDate FROM Books WHERE Title='Dated'")" "2025-03-01"

before=$(sql "SELECT COUNT(*) FROM Books")
req POST /Books/Create '{"title":"","genre":0,"priceDkk":10}'
expect "a blank title from an empty form field is not created" "$(sql "SELECT COUNT(*) FROM Books")" "$before"
req POST /Books/Create '{"title":"Negative","genre":0,"priceDkk":-1}'
expect "a negative price is not created" "$(sql "SELECT COUNT(*) FROM Books WHERE Title='Negative'")" "0"

# A text input left empty usually posts "" rather than null. A design question, so only reported.
req POST /Books/Create '{"title":"BlankIsbn","isbn":"","genre":0,"priceDkk":10}'
info "blank isbn text input, rows created" "$(sql "SELECT COUNT(*) FROM Books WHERE Title='BlankIsbn'")"
req POST /Authors/Create '{"firstName":"Blank","lastName":"Website","website":""}'
info "blank website text input, rows created" "$(sql "SELECT COUNT(*) FROM Authors WHERE FirstName='Blank'")"
