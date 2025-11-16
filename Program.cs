using ManaBoxImporter.Repositories;
using ManaBoxImporter.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

Console.WriteLine("ManaBoxImporter 4.0");

var portOption = new Option<string?>("--port", "-p")
{
    Description = "Port number on which MTGA Tracker Daemon is running.",
};

var scryfallOption = new Option<string?>("--scryfall", "-s")
{
    Description = "Path to json file containing all cards from Scryfall.",
};

var outputOption = new Option<string?>("--output", "-o")
{
    Description = "Path to the folder where the inventory file will be exported.",
};

var rootCommand = new RootCommand("ManaBoxImporter")
{
	portOption,
	scryfallOption,
	outputOption,
};

var parseResult = rootCommand.Parse(args);

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>()
    {
        ["Port"] = parseResult.GetValue(portOption),
		["ScryfallFilePath"] = parseResult.GetValue(scryfallOption),
		["OutputFilePath"] = parseResult.GetValue(outputOption),
    })
    .Build();

var services = new ServiceCollection()
    .AddSingleton<IConfiguration>(configuration)
    .AddSingleton<InventoryService>()
    .AddSingleton<TransformService>()
    .AddSingleton<OutputService>()
    .AddSingleton<MtgaTrackerDaemonRepository>()
    .AddSingleton<MtgaProRepository>()
    .AddSingleton<ScryfallRepository>()
    .AddSingleton<SeventeenLandsRepository>();

services.AddHttpClient("Scryfall", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://api.scryfall.com/");
    httpClient.DefaultRequestHeaders.Add("User-Agent", "ManaBoxImporter/4.0");
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
});

var serviceProvider = services.BuildServiceProvider();

// Let the magic begin!

var inventoryService = serviceProvider.GetRequiredService<InventoryService>();
var transformService = serviceProvider.GetRequiredService<TransformService>();
var outputService = serviceProvider.GetRequiredService<OutputService>();

var inventory = await inventoryService.GetInventory();

if (inventory == null ||
	inventory.Cards.Count == 0)
{
	Console.WriteLine("Unable to find any cards to be imported");
	Environment.Exit(-2);
}

var transformedInventory = await transformService.TransformInventory(inventory);

var outputFilePath = await outputService.Output(transformedInventory, inventory.Timestamp);

Console.WriteLine($"Export completed!");
Console.WriteLine($"CSV available at '{outputFilePath}'");