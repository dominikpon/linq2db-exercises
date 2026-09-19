# A TS client types optional columns as "string | null" and expects the key to be present.
req POST /Authors/Create '{"firstName":"Only","lastName":"Names"}'
for k in bio nationality website birthDate; do
    expect "response has key $k" "$(jq "has(\"$k\")" <<<"$BODY")" "true"
    expect "response $k is null" "$(jqb ".$k")" "null"
done
expect "db bio is NULL, not empty string" "$(sql "SELECT Bio IS NULL FROM Authors WHERE FirstName='Only'")" "1"

req POST /Authors/Create '{"firstName":"Explicit","lastName":"Nulls","bio":null,"website":null,"birthDate":null}'
expect "explicit nulls create a row" "$(sql "SELECT COUNT(*) FROM Authors WHERE FirstName='Explicit'")" "1"
