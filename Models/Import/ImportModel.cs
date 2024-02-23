namespace ManaBoxImporter.Models.Import;

public class ImportModel
{
	public List<CardImport> Cards { get; set; } = [];
	
	public string CollectionFilePathExtension { get; set; } = string.Empty;
}
