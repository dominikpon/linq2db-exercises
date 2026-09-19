for p in 0.1 19.99 1234567.89; do
    req POST /Books/Create "{\"title\":\"price-$p\",\"genre\":\"Fiction\",\"priceDkk\":$p}"
    sent=$(sed -n 's/.*"priceDkk":\([^,}]*\).*/\1/p' <<<"$BODY")
    expect "response prints $p exactly" "$sent" "$p"
    expect "db stores $p exactly" "$(sql "SELECT PriceDkk FROM Books WHERE Title='price-$p'")" "$p"
    qget /Books/GetById "id=$(jqb .id)"
    expect "GetById prints $p exactly" "$(sed -n 's/.*"priceDkk":\([^,}]*\).*/\1/p' <<<"$BODY")" "$p"
done
