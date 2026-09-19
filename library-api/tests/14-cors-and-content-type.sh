H=$(curl -s -D - -o /dev/null -X OPTIONS "$BASE/Authors/Update" -H 'Origin: http://localhost:3000' -H 'Access-Control-Request-Method: PUT' -H 'Access-Control-Request-Headers: content-type')
hdr() { grep -i "^$1:" <<<"$H" | head -1 | cut -d: -f2- | tr -d '\r' | sed 's/^ *//' | tr 'A-Z' 'a-z'; }
expect_match "preflight allows the origin" "$(hdr access-control-allow-origin)" '^(\*|http://localhost:3000)$'
expect_match "preflight allows PUT" "$(hdr access-control-allow-methods)" 'put|\*'
expect_match "preflight allows the content-type header" "$(hdr access-control-allow-headers)" 'content-type|\*'

H=$(curl -s -D - -o /dev/null "$BASE/Authors/GetAll" -H 'Origin: http://localhost:3000')
expect_match "a real request gets the CORS header too" "$(hdr access-control-allow-origin)" '^(\*|http://localhost:3000)$'
expect_match "responses are application/json" "$(hdr content-type)" '^application/json'
