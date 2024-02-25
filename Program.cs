using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using CommandLine;
using CsvHelper;
using ManaBoxImporter.Models;
using ManaBoxImporter.Models.Import;
using ShellProgressBar;

Console.WriteLine("ManaBoxImporter 1.8");

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

if (!string.IsNullOrEmpty(_options.ScryfallJsonFilePath) &&
	importModel.CollectionFilePathExtension != ".csv")
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
	MaxDegreeOfParallelism = string.IsNullOrEmpty(_options.ScryfallJsonFilePath) ? 1 : 10
};

var log = string.Empty;

var cardRecords = new ConcurrentBag<CardImport>();

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

		FixCard(card);

		progressBar.Tick($"({progressBar.CurrentTick}/{progressBar.MaxTicks}): Exporting card {card.Name}");

		cardRecords.Add(card);
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

var exportFilePath = GetExportFilePath();

using var writer = new StreamWriter(exportFilePath);
using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
csv.WriteRecords(cardRecords);

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
	=> card.Name.StartsWith("A-", StringComparison.OrdinalIgnoreCase);

void FixCard(CardImport card) 
{
	var oldSetCode = card.SetCode;
	
	card.SetCode = card.SetCode
		.Replace("AHA", "HA")
		.Replace("Y22-MID", "YMID")
		.Replace("Y23_DMU", "YDMU")
		.Replace("Y22_NEO", "YNEO")
		.Replace("Y22_SNC", "YSNC")
		.Replace("Y23_BRO", "YBRO")
		.Replace("Y23_ONE", "YONE")
		.Replace("Y24_WOE", "YWOE")
		.Replace("Y24_LCI", "YLCI");
	
	if(oldSetCode != card.SetCode) 
	{
		log += $"Fixing set code to '{card.SetCode}' for card '{card.Name}'" + Environment.NewLine;
	}
	
	var oldCardName  = card.Name;
	
	card.Name = card.Name
		.Replace("Lothlorien Lookout", "Lothlórien Lookout")
		.Replace("Arwen Undomiel", "Arwen Undómiel")
		.Replace("Bartolome del Presidio", "Bartolomé del Presidio")
		.Replace("Cathartic Re", "Cathartic Reunion");
	
	if (oldCardName != card.Name) 
	{
		log += $"Fixing card name '{card.Name}'" + Environment.NewLine;
	}
}

async Task WriteLogFile()
{
	if (_options?.EnableLogFile != true ||
		string.IsNullOrEmpty(log))
	{
		return;
	}

	var logFilePath = Path.ChangeExtension(_options!.CollectionFilePath.Trim(), "log");

	await File.WriteAllTextAsync(logFilePath, log);

	Console.WriteLine($"Log available at {logFilePath}");
}

async Task<ImportModel?> GetCollection()
{
	var extension = Path.GetExtension(_options.CollectionFilePath);
	
	if (extension == ".json")
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
			CollectionFilePathExtension = extension,
		};
	}

	if (extension == ".csv")
	{
		using var reader = new StreamReader(_options.CollectionFilePath.Trim());
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
			CollectionFilePathExtension = extension,
		};
	}

	return null;
}

string GetExportFilePath()
{
	var exportFilePath = Path.Combine(
		Path.GetDirectoryName(_options.CollectionFilePath),
		$"{Path.GetFileNameWithoutExtension(_options.CollectionFilePath)}-{Guid.NewGuid()}.csv");

	return exportFilePath.Trim();
}