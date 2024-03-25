namespace ManaBoxImporter.Models.Import;

public class MTGAProInventoryImport 
{
	public string Timestamp { get; set; } = string.Empty;
	
	public Dictionary<int, int> Payload { get; set; } = [];
}