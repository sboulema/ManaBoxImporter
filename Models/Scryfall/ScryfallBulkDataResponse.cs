namespace ManaBoxImporter.Models.Scryfall;

public class ScryfallBulkDataResponse
{
    public IEnumerable<ScryfallBulkData> Data { get; set; } = [];
}
