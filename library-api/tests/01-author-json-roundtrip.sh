# A browser form posts an author with unicode, quotes, backslashes and newlines in it.
req POST /Authors/Create '{"firstName":"Søren","lastName":"Ærø \"Q\" 日本","bio":"line1\nline2 \\ back","nationality":"Danish","website":"https://ex.com","birthDate":"1990-01-01"}'
ID=$(jqb .id)
expect "response echoes the unicode first name" "$(jqb .firstName)" "Søren"
expect "response echoes birthDate as a plain date" "$(jqb .birthDate)" "1990-01-01"
expect "db holds names byte-for-byte" "$(sql "SELECT FirstName||'|'||LastName FROM Authors WHERE FirstName='Søren'")" 'Søren|Ærø "Q" 日本'
expect "db holds bio with newline and backslash intact" "$(sql "SELECT Bio FROM Authors WHERE FirstName='Søren'")" $'line1\nline2 \\ back'
expect "db stores birthDate as yyyy-MM-dd" "$(sql "SELECT BirthDate FROM Authors WHERE FirstName='Søren'")" "1990-01-01"

qget /Authors/GetById "id=$ID"
expect "reading it back gives the same last name" "$(jqb .lastName)" 'Ærø "Q" 日本'
expect "reading it back gives the same bio" "$(jqb .bio)" $'line1\nline2 \\ back'
