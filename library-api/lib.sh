# Sourced by every test in tests/. Expects BASE (server url) and DB_FILE (sqlite path) in the env.
PASSES=0
FAILS=0
RESP="$(mktemp)"

# Seed ids, as the strings a web client would send (LibrarySeed.AuthorIdOf / BookIdOf).
aid() { printf '%08d-0000-4000-a000-000000000000' "$1"; }
bid() { printf '%08d-0000-4000-b000-000000000000' "$1"; }

# req METHOD PATH [JSON-BODY] - sets STATUS and BODY. The status is printed for context only.
req() {
    local method=$1 path=$2 body=${3-}
    if [ -n "$body" ]; then
        STATUS=$(curl -s -o "$RESP" -w '%{http_code}' -X "$method" "$BASE$path" -H 'Content-Type: application/json' -d "$body")
    else
        STATUS=$(curl -s -o "$RESP" -w '%{http_code}' -X "$method" "$BASE$path")
    fi
    BODY=$(<"$RESP")
    echo "  $method $path -> $STATUS"
}

# qget PATH key=value ... - GET with properly url-encoded query parameters.
qget() {
    local path=$1
    shift
    local args=()
    local kv
    for kv in "$@"; do args+=(--data-urlencode "$kv"); done
    STATUS=$(curl -s -G -o "$RESP" -w '%{http_code}' "$BASE$path" "${args[@]}")
    BODY=$(<"$RESP")
    echo "  GET $path $* -> $STATUS"
}

jqb() { jq -r "$1" <<<"$BODY" 2>/dev/null; }
sql() { sqlite3 -batch -noheader "$DB_FILE" "$1"; }

pass() { PASSES=$((PASSES + 1)); echo "  ok   $1"; }
fail() {
    FAILS=$((FAILS + 1))
    echo "  FAIL $1"
    echo "       expected: $3"
    echo "       actual:   $2"
}

expect() { if [ "$2" = "$3" ]; then pass "$1"; else fail "$1" "$2" "$3"; fi; }
expect_not() { if [ "$2" != "$3" ]; then pass "$1"; else fail "$1" "$2" "anything but: $3"; fi; }
expect_match() { if [[ $2 =~ $3 ]]; then pass "$1"; else fail "$1" "$2" "matching: $3"; fi; }
info() { echo "  info $1: $2"; }

finish() {
    echo "  $PASSES ok, $FAILS failed"
    rm -f "$RESP"
    [ "$FAILS" -eq 0 ]
}
