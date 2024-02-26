using CsvHelper.Configuration.Attributes;

namespace ManaBoxImporter.Models;

public class Card17Lands
{
	[Name("id")]
	public int ArenaId { get; set; }

	[Name("expansion")]
	public string SetCode { get; set; } = string.Empty;
	
	[Name("name")]
    public string Name { get; set; } = string.Empty;
}