using System.Text.Json.Serialization;

namespace ManaBoxImporter.Models.MtgaTrackerDaemon;

public class MtgaTrackerDaemonCard 
{
	[JsonPropertyName("owned")]
	public int Quantity { get; set; }

	[JsonPropertyName("grpId")]
	public int? GroupId { get; set; }

	[JsonPropertyName("title")]
	public string Title { get; set; } = string.Empty;

    [JsonPropertyName("expansionCode")]
    public string ExpansionCode { get; set; } = string.Empty;
}