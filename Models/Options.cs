using CommandLine;

namespace ManaBoxImporter.Models;

public class Options
{
    [Option('c', "collection", Required = true, HelpText = "Path to json collection file.")]
    public string CollectionFilePath { get; set; } = string.Empty;

    [Option('s', "scryfall", Required = false, HelpText = "Path to Scryfall json cards file.")]
    public string scryfallJsonFilePath { get; set; } = string.Empty;
}
