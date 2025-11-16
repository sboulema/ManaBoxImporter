namespace ManaBoxImporter.Models.Inventory;

public class InventoryModel
{
	public List<InventoryCard> Cards { get; set; } = [];
	
	public string Timestamp { get; set; } = string.Empty;
}
