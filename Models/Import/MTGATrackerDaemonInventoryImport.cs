using System.Text.Json.Serialization;

namespace ManaBoxImporter.Models.Import;

public class MTGATrackerDaemonInventoryImport 
{
	[JsonPropertyName("cards")]
	public List<MTGATrackerDaemonCardImport> Cards { get; set; } = [];
}

public class MTGATrackerDaemonCardImport 
{
	[JsonPropertyName("owned")]
	public int Quantity { get; set; }

	[JsonPropertyName("grpId")]
	public int? GroupId { get; set; }
}