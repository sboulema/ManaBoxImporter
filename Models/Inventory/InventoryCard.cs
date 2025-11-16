using CsvHelper.Configuration.Attributes;

namespace ManaBoxImporter.Models.Inventory;

public class InventoryCard
{
    public string Name { get; set; } = string.Empty;

    [Name("Set code")]
    public string SetCode { get; set; } = string.Empty;

    [Name("Set name")]
    public string SetName { get; set; } = string.Empty;

    [Name("Collector number")]
    public string CollectorNumber { get; set; } = string.Empty;

    [Name("Scryfall ID")]
    public Guid? ScryFallId { get; set; }

    public int Quantity { get; set; }

    public int? GroupId { get; set; }
}
