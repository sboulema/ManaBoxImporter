using CommandLine;

namespace ManaBoxImporter.Models;

public class Options
{
	[Option('s', "scryfall", Required = true, HelpText = "Path to Scryfall json cards file.")]
	public string ScryfallJsonFilePath { get; set; } = string.Empty;
	
	[Option('o', "output", Required = false, HelpText = "Directory path to output files.")]
    public string OutputFilePath { get; set; } = string.Empty;

	[Option('l', "log", Required = false, HelpText = "Write error log to file.")]
	public bool EnableLogFile { get; set; }
}
