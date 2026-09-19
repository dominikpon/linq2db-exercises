before=$(sql "SELECT COUNT(*) FROM Books")
req POST /Books/Create '{"title":'
info "truncated JSON" "$STATUS"
req POST /Books/Create '{"title":"y","genre":0,"priceDkk":"abc"}'
info "price as a string" "$STATUS"
req POST /Books/Create '{"title":"z","genre":"Poetry","priceDkk":1}'
info "genre as a string" "$STATUS"
req POST /Books/Create '[1,2,3]'
info "array where object expected" "$STATUS"
req POST /Books/Create 'not json at all'
info "not json" "$STATUS"
req POST /Authors/Create '{"firstName":"A","lastName":"B","birthDate":"1990-01-01T00:00:00Z"}'
info "ISO datetime into a date field" "$STATUS"

expect "none of it created a book" "$(sql "SELECT COUNT(*) FROM Books")" "$before"
expect "no author was created from the datetime-as-date body" "$(sql "SELECT COUNT(*) FROM Authors WHERE FirstName='A' AND LastName='B'")" "0"
qget /Books/Count
expect "the server is still healthy afterwards" "$BODY" "$before"
