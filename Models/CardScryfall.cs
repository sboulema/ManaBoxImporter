using System.Text.Json.Serialization;

namespace ManaBoxImporter.Models.Import
{
    public class CardScryfall
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("set")]
        public string SetCode { get; set; } = string.Empty;

        [JsonPropertyName("set_name")]
        public string SetName { get; set; } = string.Empty;

        [JsonPropertyName("collector_number")]
        public string CollectorNumber { get; set; } = string.Empty;
    }
}