using System.Text.Json.Serialization;

namespace ManaBoxImporter.Models.Import;

public class CardImportJson
{
    [JsonPropertyName("grpId")]
    public int GroupId { get; set; }

    [JsonPropertyName("owned")]
    public int Quantity { get; set; }
}

public class ImportModelJson
{
    [JsonPropertyName("cards")]
    public List<CardImportJson> Cards { get; set; } = [];
}
