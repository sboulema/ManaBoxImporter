using System.Net.Http.Json;
using System.Text.Json;
using CommandLine;
using ManaBoxImporter.Models;
using ManaBoxImporter.Models.Import;
using ShellProgressBar;

Console.WriteLine("ManaBoxImporter 1.5");

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

var log = string.Empty;

var csv = "Name,Set code,Set name,Collector number,Scryfall ID,Quantity" + Environment.NewLine;

using var progressBar = new ProgressBar(importModel.Cards.Count, "Initial message");

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
        if (IsAlchemy(cardScryfall))
        {
            log += $"Ignoring Alchemy card {cardScryfall.Name}" + Environment.NewLine;
            progressBar.Tick($"({progressBar.CurrentTick}/{progressBar.MaxTicks}): Ignoring Alchemy card {cardScryfall.Name}");
            return;
        }

        progressBar.Tick($"({progressBar.CurrentTick}/{progressBar.MaxTicks}): Exporting card {cardScryfall.Name}");

        csv += $"\"{cardScryfall.Name}\",{cardScryfall.SetCode},{cardScryfall.SetName},{cardScryfall.CollectorNumber},{cardScryfall.Id},{card.Quantity}" + Environment.NewLine;
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        Console.WriteLine($"Error exporting card {card.GroupId}");
        log += $"Error exporting card {card.GroupId}" + Environment.NewLine;
    }
    finally
    {
        if (!string.IsNullOrEmpty(_options.scryfallJsonFilePath))
        {
            Thread.Sleep(50);
        }
    }
});

progressBar.Dispose();

var exportFilePath = Path.ChangeExtension(_options.CollectionFilePath, "csv");

await File.WriteAllTextAsync(exportFilePath.Trim(), csv);

Console.WriteLine($"Export completed!");
Console.WriteLine($"CSV available at {exportFilePath}");

await WriteLogFile();

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

bool IsAlchemy(CardScryfall card) {
    var alchemySets = new List<string>{ "y22", "ymid", "yneo" };

    return card.Name.StartsWith("A-") || alchemySets.Contains(card.SetCode);
}

async Task WriteLogFile() {
    if (_options?.enableLogFile != true ||
        string.IsNullOrEmpty(log)) {
        return;
    }

    var logFilePath = Path.ChangeExtension(_options!.CollectionFilePath, "log");

    await File.WriteAllTextAsync(logFilePath.Trim(), log);

    Console.WriteLine($"Log available at {logFilePath}");
}