using CommandLine;

namespace ManaBoxImporter.Models;

public class Options
{
	[Option('p', "port", Required = false, HelpText = "Port on which the mtga-tracker-daemon is running.")]
	public int? Port { get; set; }
	
	[Option('s', "scryfall", Required = false, HelpText = "Path to Scryfall json cards file.")]
	public string ScryfallJsonFilePath { get; set; } = string.Empty;
	
	[Option('o', "output", Required = false, HelpText = "Directory path to output files.")]
	public string OutputFilePath { get; set; } = string.Empty;
}
