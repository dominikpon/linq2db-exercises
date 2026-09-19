using LinqToDB.Mapping;

namespace Infa;

/// <summary>SQLite stores no time zone, so a DateTime reads back as Kind=Unspecified and serializes without a Z.</summary>
public class UtcDateTimeConverter() : ValueConverterFunc<DateTime, DateTime>(
    model => model,
    provider => DateTime.SpecifyKind(provider, DateTimeKind.Utc),
    false);
