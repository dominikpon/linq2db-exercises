#!/usr/bin/env bash
# Runs every test folder under tests/ one at a time and prints a summary.
set -uo pipefail

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PASS=0
FAIL=0

for t in "$DIR"/tests/*/; do
    if "$DIR/run-test.sh" "$t"; then
        PASS=$((PASS + 1))
    else
        FAIL=$((FAIL + 1))
    fi
done

echo
echo "$PASS passed, $FAIL failed"
[ "$FAIL" -eq 0 ]
