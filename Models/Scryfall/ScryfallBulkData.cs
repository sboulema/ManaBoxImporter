using System.Text.Json.Serialization;

namespace ManaBoxImporter.Models.Scryfall;

public class ScryfallBulkData
{
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("download_uri")]
    public string DownloadUri { get; set; } = string.Empty;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
