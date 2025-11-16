namespace ManaBoxImporter.Models.MtgaPro;

public class MtgaProInventoryImport 
{
	public string Timestamp { get; set; } = string.Empty;
	
	public Dictionary<int, int> Payload { get; set; } = [];
}