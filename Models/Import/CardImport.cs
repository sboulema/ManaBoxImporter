using System.Text.Json.Serialization;

namespace ManaBoxImporter.Models.Import;

public class CardImport
{
    [JsonPropertyName("grpId")]
    public int GroupId { get; set; }

    [JsonPropertyName("owned")]
    public int Quantity { get; set; }
}
