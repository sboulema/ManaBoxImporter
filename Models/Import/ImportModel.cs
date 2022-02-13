using System.Text.Json.Serialization;

namespace ManaBoxImporter.Models.Import
{
    public class ImportModel
    {
        [JsonPropertyName("cards")]
        public List<CardImport> Cards { get; set; } = new();
    }
}