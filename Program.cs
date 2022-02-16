using System.Net.Http.Json;
using System.Text.Json;
using CommandLine;
using ManaBoxImporter.Models;
using ManaBoxImporter.Models.Import;

Console.WriteLine("ManaBoxImporter 1.2");

var _options = new Options();

Parser.Default
    .ParseArguments<Options>(args)
    .WithParsed<Options>(o => _options = o);

var importModel = await ParseFile<ImportModel>(_options.CollectionFilePath);

if (importModel?.Cards?.Any() != true) {
    Console.WriteLine("Unable to find any cards to be imported");
    Environment.Exit(-2);
}

var scryfallCards = new List<CardScryfall>();

if (!string.IsNullOrEmpty(_options.scryfallJsonFilePath))
{
    Console.WriteLine("Loading Scryfall database from file");
    scryfallCards = await ParseFile<List<CardScryfall>>(_options.scryfallJsonFilePath);
}

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.scryfall.com/")
};

var parallelOptions = new ParallelOptions
{
    MaxDegreeOfParallelism = string.IsNullOrEmpty(_options.scryfallJsonFilePath) ? 1 : 3
};

var lockTarget = new object();

var progress = 1;

var csv = "Name,Set code,Set name,Collector number,Scryfall ID,Quantity" + Environment.NewLine;

await Parallel.ForEachAsync(importModel.Cards, parallelOptions, async (card, cancellationToken) =>
{
    try
    {
        var cardScryfall = await GetCard(card.GroupId, scryfallCards, httpClient);

        if (cardScryfall == null)
        {
            return;
        }

        // Ignore Alchemy cards
        if (cardScryfall.Name.StartsWith("A-") ||
            cardScryfall.SetCode.Equals("y22"))
        {
            Console.WriteLine($"({progress}/{importModel.Cards.Count}): Ignoring Alchemy card {cardScryfall.Name}");
            return;
        }

        Console.WriteLine($"({progress}/{importModel.Cards.Count}): Exporting card {cardScryfall.Name}");

        csv += $"\"{cardScryfall.Name}\",{cardScryfall.SetCode},{cardScryfall.SetName},{cardScryfall.CollectorNumber},{cardScryfall.Id},{card.Quantity}" + Environment.NewLine;
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        Console.WriteLine($"Error exporting card {card.GroupId}");
    }
    finally
    {
        lock (lockTarget) {
            progress++;
        }

        if (!string.IsNullOrEmpty(_options.scryfallJsonFilePath))
        {
            Thread.Sleep(50);
        }
    }
});

var exportFilePath = Path.ChangeExtension(_options.CollectionFilePath, "csv");

await File.WriteAllTextAsync(exportFilePath.Trim(), csv);

Console.WriteLine($"Export completed!");
Console.WriteLine($"CSV available at {exportFilePath}");

async Task<T?> ParseFile<T>(string path)
    => JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(path.Trim()));

async Task<CardScryfall?> GetCard(int arenaId, List<CardScryfall>? cards, HttpClient httpClient)
{
    if (cards?.Any() != true)
    {
        return await httpClient.GetFromJsonAsync<CardScryfall>($"cards/arena/{arenaId}");
    }

    return cards.FirstOrDefault(card => card.ArenaId == arenaId);
}