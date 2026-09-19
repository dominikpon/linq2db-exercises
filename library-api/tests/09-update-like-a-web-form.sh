A=$(aid 1)

# --- PATCH: send only what changed; null/omitted means "leave alone" ---
req PATCH /Authors/Update "{\"id\":\"$A\",\"firstName\":\"Renamed\"}"
expect "PATCH changed only the named column" "$(sql "SELECT FirstName||'|'||LastName||'|'||Website FROM Authors WHERE LastName='Marsh'")" "Renamed|Marsh|https://elenamarsh.example.com"

req PATCH /Authors/Update "{\"id\":\"$A\",\"website\":null}"
expect "PATCH with an explicit null leaves the column alone" "$(sql "SELECT Website FROM Authors WHERE LastName='Marsh'")" "https://elenamarsh.example.com"
req PATCH /Authors/Update "{\"id\":\"$A\",\"firstNmae\":\"Typo\"}"
info "PATCH with a typo'd property name, status" "$STATUS"

# --- PUT: the classic edit form. GET the object, change something, PUT the whole thing back ---
T=$(aid 2)
qget /Authors/GetById "id=$T"
before=$(sql "SELECT FirstName||'|'||LastName||'|'||IFNULL(Bio,'-')||'|'||CreatedAtUtc FROM Authors WHERE LastName='Reyne'")
req PUT /Authors/Replace "$BODY"
expect "PUT of a fetched author, unchanged, changes nothing" "$(sql "SELECT FirstName||'|'||LastName||'|'||IFNULL(Bio,'-')||'|'||CreatedAtUtc FROM Authors WHERE LastName='Reyne'")" "$before"

qget /Authors/GetById "id=$A"
req PUT /Authors/Replace "$(jq -c '.website = null | .bio = null | .firstName = "Elena"' <<<"$BODY")"
expect "PUT with null clears the nullable columns" "$(sql "SELECT (Website IS NULL)||(Bio IS NULL) FROM Authors WHERE LastName='Marsh'")" "11"
expect "PUT changed the sent first name" "$(sql "SELECT FirstName FROM Authors WHERE LastName='Marsh'")" "Elena"
req PUT /Authors/Replace "$(jq -c '.website = "https://again.example.com"' <<<"$BODY")"
expect "PUT can set the column again" "$(sql "SELECT Website FROM Authors WHERE LastName='Marsh'")" "https://again.example.com"

req PUT /Authors/Replace "$(jq -c '.website = ""' <<<"$BODY")"
info "PUT with a blank website text input, db now" "$(sql "SELECT IFNULL(Website,'<NULL>') FROM Authors WHERE LastName='Marsh'")"

qget /Books/GetById "id=$(bid 3)"
before=$(sql "SELECT Title||'|'||Genre||'|'||PriceDkk||'|'||IFNULL(Isbn,'-')||'|'||IsOutOfPrint FROM Books WHERE Title='Cinder and Salt'")
req PUT /Books/Replace "$BODY"
expect "PUT of a fetched book, unchanged, changes nothing" "$(sql "SELECT Title||'|'||Genre||'|'||PriceDkk||'|'||IFNULL(Isbn,'-')||'|'||IsOutOfPrint FROM Books WHERE Title='Cinder and Salt'")" "$before"

req PUT /Books/Replace "$(jq -c '.publishedDate = null | .isOutOfPrint = false' <<<"$BODY")"
expect "PUT clears publishedDate and puts the book back in print" "$(sql "SELECT (PublishedDate IS NULL)||IsOutOfPrint FROM Books WHERE Title='Cinder and Salt'")" "10"

req PATCH /Books/Update "{\"id\":\"$(bid 3)\",\"priceDkk\":42.5}"
expect "PATCH changes just the price" "$(sql "SELECT PriceDkk||'|'||Title FROM Books WHERE Title='Cinder and Salt'")" "42.5|Cinder and Salt"
