#!/usr/bin/env bash
# Black-box tests for the Library API. Each test file gets a fresh SQLite file and a freshly
# started server (the app seeds the library itself on startup). Tests talk to it with curl and
# look at the database with sqlite3 - they never touch C#.
#   ./library-api/run.sh                 # all tests
#   ./library-api/run.sh 03-update       # tests whose name starts with this
set -uo pipefail

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$DIR/.." && pwd)"
PORT=5298
export BASE="http://127.0.0.1:$PORT"

dotnet build "$ROOT/API/API.csproj" -nologo -v q >/tmp/library-api-build.log 2>&1 || { cat /tmp/library-api-build.log; exit 1; }

run_one() {
    local t=$1 name db log pid rc ready=false
    name=$(basename "$t" .sh)
    db=$(mktemp -u "/tmp/library-api-XXXXXX.db")
    log=$(mktemp -u "/tmp/library-api-XXXXXX.log")

    DB="Data Source=$db" ASPNETCORE_URLS="$BASE" \
        dotnet run --project "$ROOT/API/API.csproj" --no-build --no-launch-profile >"$log" 2>&1 &
    pid=$!
    for _ in $(seq 1 30); do
        if curl -s -o /dev/null "$BASE/Authors/Count"; then ready=true; break; fi
        sleep 1
    done

    echo "== $name"
    if $ready; then
        ( export DB_FILE="$db"; source "$DIR/lib.sh"; source "$t"; finish )
        rc=$?
    else
        echo "  server never came up - see $log"
        rc=1
    fi

    pkill -9 -P "$pid" >/dev/null 2>&1
    kill -9 "$pid" >/dev/null 2>&1
    rm -f "$db"
    [ "$rc" -eq 0 ] && rm -f "$log"
    return "$rc"
}

tests=()
if [ $# -eq 0 ]; then
    tests=("$DIR"/tests/*.sh)
else
    for prefix in "$@"; do tests+=("$DIR"/tests/"$prefix"*.sh); done
fi

failed=()
for t in "${tests[@]}"; do
    run_one "$t" || failed+=("$(basename "$t" .sh)")
done

echo
if [ ${#failed[@]} -eq 0 ]; then
    echo "all ${#tests[@]} test files clean"
else
    echo "${#failed[@]} of ${#tests[@]} test files have failing checks:"
    printf '  %s\n' "${failed[@]}"
    exit 1
fi
