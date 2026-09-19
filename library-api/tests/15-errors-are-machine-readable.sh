# A web client needs to show something useful when the server says no.
show() {
    local label=$1
    expect "$label: body is JSON" "$(jq -e . >/dev/null 2>&1 <<<"$BODY" && echo yes || echo no)" "yes"
    expect_match "$label: has a human-readable message" "$(jqb '.title // .detail // .message // empty')" '.+'
}
req GET "/Authors/GetById?id=$(aid 999)"; show "unknown id"
qget /Authors/Search q=a; show "validation failure"
req DELETE "/Authors/Delete?id=$(aid 1)"; show "conflict"
req POST /Authors/Create '{"firstName":"","lastName":""}'; show "blank required fields"
info "which field was wrong is reported as" "$(jqb 'keys | join(",")')"
