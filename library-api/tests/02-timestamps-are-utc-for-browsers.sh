# JS parses "2024-01-01T09:00:00" as LOCAL time and "...Z" as UTC. CreatedAtUtc must say Z everywhere.
req POST /Authors/Create '{"firstName":"Tim","lastName":"Stamp"}'
ID=$(jqb .id)
expect_match "create response createdAtUtc ends in Z" "$(jqb .createdAtUtc)" 'Z$'
qget /Authors/GetById "id=$ID"
expect_match "GetById createdAtUtc ends in Z" "$(jqb .createdAtUtc)" 'Z$'
qget /Authors/GetById "id=$(aid 1)"
expect_match "seeded row's createdAtUtc ends in Z" "$(jqb .createdAtUtc)" 'Z$'
qget /Books/GetAll
expect_match "list endpoint timestamps end in Z" "$(jqb '.[0].createdAtUtc')" 'Z$'
