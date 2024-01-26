using CsvHelper.Configuration.Attributes;

namespace ManaBoxImporter.Models.Import;

public class CardImportCsv
{
    [Name("Card")]
    public string CardName { get; set; } = string.Empty;

    [Name("Set ID")]
    public string SetId { get; set; } = string.Empty;

    [Name("Set Name")]
    public string SetName { get; set; } = string.Empty;

    public int Quantity { get; set; }
}
