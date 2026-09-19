A=$(aid 11); B=$(bid 12)
links() { sql "SELECT COUNT(*) FROM AuthorBooks WHERE AuthorId=(SELECT Id FROM Authors WHERE LastName='Kemp') AND BookId=(SELECT Id FROM Books WHERE Title='Unwritten Kingdoms')"; }

req POST "/AuthorBooks/Link?authorId=$A&bookId=$B"
expect "link row exists" "$(links)" "1"
req POST "/AuthorBooks/Link?authorId=$A&bookId=$B"
expect "linking twice still leaves one row" "$(links)" "1"

qget /AuthorBooks/IsLinked "authorId=$A" "bookId=$B"
expect "IsLinked returns a bare JSON boolean" "$BODY" "true"
qget /Authors/GetForBook "bookId=$B"
expect "author list for the book" "$(jqb '.[].lastName')" "Kemp"
qget /Books/GetByAuthor "authorId=$A"
expect "book list for the author" "$(jqb '.[].title')" "Unwritten Kingdoms"

req DELETE "/AuthorBooks/Unlink?authorId=$A&bookId=$B"
expect "link row removed" "$(links)" "0"
req DELETE "/AuthorBooks/Unlink?authorId=$A&bookId=$B"
expect "unlinking twice is harmless" "$(links)" "0"
qget /AuthorBooks/IsLinked "authorId=$A" "bookId=$B"
expect "IsLinked is false again" "$BODY" "false"
