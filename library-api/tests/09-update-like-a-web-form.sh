A=$(aid 1)
req PUT /Authors/Update "{\"id\":\"$A\",\"firstName\":\"Renamed\"}"
expect "only the named column changed" "$(sql "SELECT FirstName||'|'||LastName||'|'||Website FROM Authors WHERE LastName='Marsh'")" "Renamed|Marsh|https://elenamarsh.example.com"

# The classic edit form: GET the object, change nothing, PUT the whole thing back.
T=$(aid 2)
qget /Authors/GetById "id=$T"
before=$(sql "SELECT FirstName||'|'||LastName||'|'||IFNULL(Bio,'-')||'|'||CreatedAtUtc FROM Authors WHERE LastName='Reyne'")
req PUT /Authors/Update "$BODY"
expect "putting a fetched author straight back changes nothing" "$(sql "SELECT FirstName||'|'||LastName||'|'||IFNULL(Bio,'-')||'|'||CreatedAtUtc FROM Authors WHERE LastName='Reyne'")" "$before"

qget /Books/GetById "id=$(bid 3)"
before=$(sql "SELECT Title||'|'||Genre||'|'||PriceDkk||'|'||IFNULL(Isbn,'-') FROM Books WHERE Title='Cinder and Salt'")
req PUT /Books/Update "$BODY"
expect "putting a fetched book straight back changes nothing" "$(sql "SELECT Title||'|'||Genre||'|'||PriceDkk||'|'||IFNULL(Isbn,'-') FROM Books WHERE Title='Cinder and Salt'")" "$before"

# Things a form does when the user blanks a field. Design questions, so just reported.
req PUT /Authors/Update "{\"id\":\"$A\",\"website\":null}"
info "explicit null website, db now" "$(sql "SELECT IFNULL(Website,'<NULL>') FROM Authors WHERE LastName='Marsh'")"
req PUT /Authors/Update "{\"id\":\"$A\",\"website\":\"\"}"
info "empty-string website, db now" "$(sql "SELECT IFNULL(Website,'<NULL>') FROM Authors WHERE LastName='Marsh'")"
req PUT /Authors/Update "{\"id\":\"$A\",\"firstNmae\":\"Typo\"}"
info "typo'd property name, status" "$STATUS"
