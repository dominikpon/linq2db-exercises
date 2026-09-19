# A client echoing a whole object back must not be able to choose id or creation time.
FAKE=11111111-1111-4111-8111-111111111111
req POST /Authors/Create "{\"id\":\"$FAKE\",\"createdAtUtc\":\"1999-01-01T00:00:00Z\",\"firstName\":\"Sneaky\",\"lastName\":\"Client\"}"
expect_not "server generated its own id" "$(jqb .id)" "$FAKE"
expect_not "server stamped its own creation time" "$(jqb .createdAtUtc | cut -c1-4)" "1999"

req POST /Books/Create '{"title":"Sneaky","genre":"Fiction","priceDkk":10,"isOutOfPrint":true}'
expect "new book starts in print whatever the client says" "$(sql "SELECT IsOutOfPrint FROM Books WHERE Title='Sneaky'")" "0"
