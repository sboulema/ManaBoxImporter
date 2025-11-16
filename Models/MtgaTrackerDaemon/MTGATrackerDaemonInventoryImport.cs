using System.Text.Json.Serialization;

namespace ManaBoxImporter.Models.MtgaTrackerDaemon;

public class MTGATrackerDaemonInventoryImport 
{
	[JsonPropertyName("cards")]
	public IEnumerable<MtgaTrackerDaemonCard> Cards { get; set; } = [];
}
