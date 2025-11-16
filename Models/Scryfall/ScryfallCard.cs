using System.Text.Json.Serialization;

namespace ManaBoxImporter.Models.Scryfall;

public class ScryfallCard
{
	[JsonPropertyName("id")]
	public Guid Id { get; set; }

	[JsonPropertyName("arena_id")]
	public int ArenaId { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("set")]
	public string SetCode { get; set; } = string.Empty;

	[JsonPropertyName("set_name")]
	public string SetName { get; set; } = string.Empty;
	
	[JsonPropertyName("set_type")]
    public string SetType { get; set; } = string.Empty;

	[JsonPropertyName("collector_number")]
	public string CollectorNumber { get; set; } = string.Empty;
}