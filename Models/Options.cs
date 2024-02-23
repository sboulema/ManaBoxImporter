using CommandLine;

namespace ManaBoxImporter.Models;

public class Options
{
    [Option('c', "collection", Required = true, HelpText = "Path to json/csv collection file.")]
    public string CollectionFilePath { get; set; } = string.Empty;

    [Option('s', "scryfall", Required = false, HelpText = "Path to Scryfall json cards file.")]
    public string ScryfallJsonFilePath { get; set; } = string.Empty;

    [Option('l', "log", Required = false, HelpText = "Write error log to file.")]
    public bool EnableLogFile { get; set; }
}
