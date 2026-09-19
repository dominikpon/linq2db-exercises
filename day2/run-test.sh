#!/usr/bin/env bash
# Runs one black-box test folder against a freshly-started API instance pointed at a
# brand-new, empty SQLite file. Usage: ./run-test.sh tests/<name>
set -uo pipefail

TEST_DIR="${1:?usage: run-test.sh <test-dir>}"
TEST_NAME="$(basename "$TEST_DIR")"
PORT=5299
BASE_URL="http://127.0.0.1:$PORT"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DB_FILE="$(mktemp -u "/tmp/day2-${TEST_NAME}-XXXXXX.db")"
SERVER_LOG="$(mktemp -u "/tmp/day2-${TEST_NAME}-server-XXXXXX.log")"
RESP_BODY="$(mktemp)"

cleanup() {
    if [ -n "${SERVER_PID:-}" ]; then
        pkill -9 -P "$SERVER_PID" >/dev/null 2>&1
        kill -9 "$SERVER_PID" >/dev/null 2>&1
    fi
    rm -f "$DB_FILE" "$SERVER_LOG" "$RESP_BODY"
}
trap cleanup EXIT

fail() {
    echo "FAIL: $TEST_NAME - $1"
    [ -f "$RESP_BODY" ] && echo "  response body: $(cat "$RESP_BODY")"
    exit 1
}

# Start the server against a fresh, empty database. Table creation still happens in the
# student's own startup code (CreateTable<T>, same as GrocerySeed/LibrarySeed) - only the
# row-level seeding below bypasses C# entirely.
DB="Data Source=$DB_FILE" ASPNETCORE_URLS="$BASE_URL" \
    dotnet run --project "$REPO_ROOT/API/API.csproj" --no-launch-profile \
    >"$SERVER_LOG" 2>&1 &
SERVER_PID=$!

READY=false
for _ in $(seq 1 30); do
    if curl -s -o /dev/null "$BASE_URL/Authors/Count"; then
        READY=true
        break
    fi
    sleep 1
done
$READY || fail "server never came up - see $SERVER_LOG"

if [ -s "$TEST_DIR/seed.sql" ]; then
    sqlite3 "$DB_FILE" < "$TEST_DIR/seed.sql" || fail "seed.sql failed to apply"
fi

REQUEST_FILE="$TEST_DIR/request.http"
[ -f "$REQUEST_FILE" ] || fail "missing request.http"
METHOD=$(head -1 "$REQUEST_FILE" | awk '{print $1}')
URL=$(head -1 "$REQUEST_FILE" | awk '{print $2}')
BODY=$(awk 'f{print} /^\r?$/{f=1}' "$REQUEST_FILE")

if [ -n "$BODY" ]; then
    STATUS=$(curl -s -o "$RESP_BODY" -w "%{http_code}" -X "$METHOD" "$URL" -H "Content-Type: application/json" -d "$BODY")
else
    STATUS=$(curl -s -o "$RESP_BODY" -w "%{http_code}" -X "$METHOD" "$URL")
fi

EXPECT_FILE="$TEST_DIR/expect.json"
[ -f "$EXPECT_FILE" ] || fail "missing expect.json"
EXPECTED_STATUS=$(jq -r '.status' "$EXPECT_FILE")
JQ_EXPR=$(jq -r '.jq' "$EXPECT_FILE")

[ "$STATUS" = "$EXPECTED_STATUS" ] || fail "expected status $EXPECTED_STATUS, got $STATUS"
if [ "$JQ_EXPR" != "null" ]; then
    jq -e "$JQ_EXPR" "$RESP_BODY" >/dev/null 2>&1 || fail "jq assertion failed: $JQ_EXPR"
fi

echo "PASS: $TEST_NAME"
