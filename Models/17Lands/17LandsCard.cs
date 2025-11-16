using CsvHelper.Configuration.Attributes;

namespace ManaBoxImporter.Models.SeventeenLands;

public class SeventeenLandsCard
{
    [Name("id")]
    public int ArenaId { get; set; }

    [Name("expansion")]
    public string SetCode { get; set; } = string.Empty;

    [Name("name")]
    public string Name { get; set; } = string.Empty;
}
