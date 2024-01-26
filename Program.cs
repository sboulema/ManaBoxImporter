using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using CommandLine;
using CsvHelper;
using ManaBoxImporter.Models;
using ManaBoxImporter.Models.Import;
using ShellProgressBar;

Console.WriteLine("ManaBoxImporter 1.6");

var _options = new Options();

Parser.Default
    .ParseArguments<Options>(args)
    .WithParsed(o => _options = o);

var importModel = await GetCollection();

if (importModel == null ||
    importModel.Cards.Count == 0) {
    Console.WriteLine("Unable to find any cards to be imported");
    Environment.Exit(-2);
}

var scryfallCards = new List<CardScryfall>();

if (!string.IsNullOrEmpty(_options.ScryfallJsonFilePath))
{
    Console.WriteLine("Loading Scryfall database from file");
    scryfallCards = await ParseFile<List<CardScryfall>>(_options.ScryfallJsonFilePath);
}

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.scryfall.com/")
};

var parallelOptions = new ParallelOptions
{
    MaxDegreeOfParallelism = string.IsNullOrEmpty(_options.ScryfallJsonFilePath) ? 1 : 3
};

var log = string.Empty;

var csv = "Name,Set code,Set name,Collector number,Scryfall ID,Quantity" + Environment.NewLine;

using var progressBar = new ProgressBar(importModel.Cards.Count, "Initial message");

await Parallel.ForEachAsync(importModel.Cards, parallelOptions, async (card, cancellationToken) =>
{
    try
    {
        if (card.GroupId != null)
        {
            var cardScryfall = await GetCard(card.GroupId.Value, scryfallCards, httpClient);

            if (cardScryfall == null)
            {
                return;
            }

            card.Name = cardScryfall.Name;
            card.SetCode = cardScryfall.SetCode;
            card.SetName = cardScryfall.SetName;
            card.CollectorNumber = cardScryfall.CollectorNumber;
            card.ScryFallId = cardScryfall.Id;
        }

        // Ignore Alchemy cards
        if (IsAlchemy(card))
        {
            log += $"Ignoring Alchemy card {card.Name}" + Environment.NewLine;
            progressBar.Tick($"({progressBar.CurrentTick}/{progressBar.MaxTicks}): Ignoring Alchemy card {card.Name}");
            return;
        }

        progressBar.Tick($"({progressBar.CurrentTick}/{progressBar.MaxTicks}): Exporting card {card.Name}");

        csv += $"\"{card.Name}\",{card.SetCode},{card.SetName},{card.CollectorNumber},{card.ScryFallId},{card.Quantity}" + Environment.NewLine;
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        Console.WriteLine($"Error exporting card {card.GroupId}");
        log += $"Error exporting card {card.GroupId}" + Environment.NewLine;
    }
    finally
    {
        if (!string.IsNullOrEmpty(_options.ScryfallJsonFilePath))
        {
            Thread.Sleep(50);
        }
    }
});

progressBar.Dispose();

var exportFilePath = GetExportFilePath(importModel);

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

bool IsAlchemy(CardImport card)
{
    if (card.SetCode.StartsWith("Y22", StringComparison.OrdinalIgnoreCase) ||
        card.SetCode.StartsWith("Y23", StringComparison.OrdinalIgnoreCase) ||
        card.SetCode.StartsWith("Y24", StringComparison.OrdinalIgnoreCase) ||
        card.SetCode.StartsWith("AHA", StringComparison.OrdinalIgnoreCase) ||
        card.Name.StartsWith("A-", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    return false;
}

async Task WriteLogFile() {
    if (_options?.EnableLogFile != true ||
        string.IsNullOrEmpty(log)) {
        return;
    }

    var logFilePath = Path.ChangeExtension(_options!.CollectionFilePath, "log");

    await File.WriteAllTextAsync(logFilePath.Trim(), log);

    Console.WriteLine($"Log available at {logFilePath}");
}

async Task<ImportModel?> GetCollection()
{
    if (!string.IsNullOrEmpty(_options.CollectionFilePath))
    {
        var json = await File.ReadAllTextAsync(_options.CollectionFilePath.Trim());
        var importModel = JsonSerializer.Deserialize<ImportModelJson>(json);
        return new()
        {
            Cards = importModel?.Cards
                .Select(card => new CardImport
                {
                    GroupId = card.GroupId,
                    Quantity = card.Quantity
                })
                .ToList() ?? [],
            CollectionFilePath = _options.CollectionFilePath
        };
    }

    if (!string.IsNullOrEmpty(_options.CSVCollectionFilePath))
    {
        using var reader = new StreamReader(_options.CSVCollectionFilePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        return new()
        {
            Cards = csv
                .GetRecords<CardImportCsv>()
                .Select(card => new CardImport
                {
                    Name = card.CardName,
                    SetCode = card.SetId,
                    SetName = card.SetName,
                    Quantity = card.Quantity
                })
                .ToList(),
            CollectionFilePath = _options.CSVCollectionFilePath
        };
    }

    return null;
}

string GetExportFilePath(ImportModel importModel)
{
    var exportFilePath = Path.Combine(
        Path.GetDirectoryName(importModel.CollectionFilePath),
        $"{Path.GetFileNameWithoutExtension(importModel.CollectionFilePath)}-{Guid.NewGuid()}.csv");

    return exportFilePath;
}