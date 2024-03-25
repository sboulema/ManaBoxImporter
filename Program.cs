using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;
using CsvHelper;
using ManaBoxImporter.Models;
using ManaBoxImporter.Models.Import;

Console.WriteLine("ManaBoxImporter 2.0");

var _options = new Options();

Parser.Default
	.ParseArguments<Options>(args)
	.WithParsed(o => _options = o);
	
var _jsonSerializerOptions = new JsonSerializerOptions 
{
	PropertyNameCaseInsensitive = true,
	NumberHandling = JsonNumberHandling.AllowReadingFromString
};

var importModel = await GetCollection();

if (importModel == null ||
	importModel.Cards.Count == 0) {
	Console.WriteLine("Unable to find any cards to be imported");
	Environment.Exit(-2);
}

var scryfallCards = GetScryfallCards();

if (scryfallCards?.Any() != true) 
{
	Console.WriteLine("Unable to load Scryfall database");
	Environment.Exit(-2);
}

var cards17Lands = await Get17LandsCards();

if (cards17Lands?.Any() != true) 
{
	Console.WriteLine("Unable to load 17Lands database");
	Environment.Exit(-2);
}

var log = string.Empty;

var collection = importModel.Cards
	.Join(cards17Lands,
		collectionCard => collectionCard.GroupId,
		card17Lands => card17Lands.ArenaId,
		(collectionCard, cards17Lands) => new CardImport 
		{
			GroupId = collectionCard.GroupId,
			Name = cards17Lands.Name,
			Quantity = collectionCard.Quantity,
		})
	.GroupJoin(
		scryfallCards,
		collectionCard => collectionCard.GroupId,
		scryfallCards => scryfallCards.ArenaId, 
		(collectionCard, scryfallCard) => new
		{
			CollectionCard = collectionCard,
			ScryfallCard = scryfallCard,
		})
	.SelectMany(
		cardMatch => cardMatch.ScryfallCard.DefaultIfEmpty(),
		(cardMatch, scryfallCard) => new CardImport 
		{
			GroupId = cardMatch.CollectionCard.GroupId,
			Name = cardMatch.CollectionCard.Name,
			Quantity = cardMatch.CollectionCard.Quantity,
			SetCode = scryfallCard?.SetCode ?? string.Empty,
			SetName = scryfallCard?.SetName ?? string.Empty,
			CollectorNumber = scryfallCard?.CollectorNumber ?? string.Empty,
			ScryFallId = scryfallCard?.Id,
		}
	)
	.Where(card => !IsAlchemy(card));
		
var alchemyCollection = collection
	.Where(card => card.ScryFallId == null)
	.Join(scryfallCards.Where(scryfallCard => scryfallCard.SetType == "alchemy"),
		collectionCard => collectionCard.Name,
		scryfallCard => scryfallCard.Name,
		(collectionCard, scryfallCard) => new CardImport 
		{
			GroupId = collectionCard.GroupId,
			Name = collectionCard.Name,
			Quantity = collectionCard.Quantity,
			SetCode = scryfallCard.SetCode,
			SetName = scryfallCard.SetName,
			CollectorNumber = scryfallCard.CollectorNumber,
			ScryFallId = scryfallCard.Id,
		});

collection = collection
	.Where(collectionCard => collectionCard.ScryFallId != null)
	.Concat(alchemyCollection)
	.DistinctBy(collectioncard => collectioncard.ScryFallId);

var outputFilePath = GetOutputFilePath(importModel.Timestamp);

using var writer = new StreamWriter(outputFilePath);
using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
csv.WriteRecords(collection);

Console.WriteLine($"Export completed!");
Console.WriteLine($"CSV available at '{outputFilePath}'");

await WriteLogFile(outputFilePath);

bool IsAlchemy(CardImport card)
	=> card.Name.StartsWith("A-", StringComparison.OrdinalIgnoreCase);

async Task WriteLogFile(string outputFilePath)
{
	if (_options?.EnableLogFile != true ||
		string.IsNullOrEmpty(log))
	{
		return;
	}

	var logFilePath = Path.ChangeExtension(outputFilePath, "log");

	await File.WriteAllTextAsync(logFilePath, log);

	Console.WriteLine($"Log available at '{logFilePath}'");
}

async Task<ImportModel?> GetCollection()
{
	if (_options.Port != null) 
	{
		return await GetMtgaTrackerDaemonCollection();
	}
	
	var playerLogPath = "%appdata%\\..\\LocalLow\\Wizards Of The Coast\\MTGA\\Player.log";
	playerLogPath = Environment.ExpandEnvironmentVariables(playerLogPath);
	var playerLogLines = await File.ReadAllLinesAsync(playerLogPath);
	var inventoryLine = playerLogLines.FirstOrDefault(line => line.StartsWith("[MTGA.Pro Logger] **Collection**"));
	
	if (string.IsNullOrEmpty(inventoryLine)) 
	{
		Console.WriteLine("Unable to find inventory in player log");
		return new();
	}
	
	var inventoryJson = inventoryLine.Replace("[MTGA.Pro Logger] **Collection**", string.Empty);
	var inventory = JsonSerializer.Deserialize<MTGAProInventoryImport>(inventoryJson, _jsonSerializerOptions);
	
	if (inventory == null) 
	{
		return new();
	}
	
	if (!string.IsNullOrEmpty(inventory.Timestamp)) 
	{
		var inventoryTimestamp =
			new DateTime(1965, 1, 1, 0, 0, 0, 0)
			.AddSeconds(double.Parse(inventory.Timestamp));
		Console.WriteLine($"Inventory timestamp: '{inventoryTimestamp}'");
	}
	
	return new() 
	{
		Cards = inventory.Payload
			.Select(card => new CardImport 
			{
				GroupId = card.Key,
				Quantity = card.Value
			})
			.ToList() ?? [],
		Timestamp = inventory.Timestamp 
	};
}

async Task<ImportModel?> GetMtgaTrackerDaemonCollection()
{
	var inventory = await new HttpClient().GetFromJsonAsync<MTGATrackerDaemonInventoryImport>($"http://localhost:{_options.Port}/cards");
	
	if (inventory == null) 
	{
		return new();
	}
	
	return new() 
	{
		Cards = inventory.Cards
			.Select(card => new CardImport 
			{
				GroupId = card.GroupId,
				Quantity = card.Quantity,
			})
			.ToList() ?? [],
		Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
	};
}

string GetOutputFilePath(string timestamp)
{
	if (!string.IsNullOrEmpty(_options.OutputFilePath)) 
	{
		return Path.Combine(Path.GetDirectoryName(_options.OutputFilePath.Trim())!, $"collection-{timestamp}.csv") ;
	}
	
	return Path.GetTempFileName();
}

async Task<IEnumerable<Card17Lands>> Get17LandsCards() 
{
	Console.WriteLine("Loading 17Lands database");
	var stream = await new HttpClient().GetStreamAsync("https://17lands-public.s3.amazonaws.com/analysis_data/cards/cards.csv");
	using var reader = new StreamReader(stream);
	using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
	return csv.GetRecords<Card17Lands>().ToList();
}

IEnumerable<CardScryfall> GetScryfallCards() 
{
	Console.WriteLine("Loading Scryfall database from file");
	using FileStream stream = File.OpenRead(_options.ScryfallJsonFilePath.Trim());
	return JsonSerializer.Deserialize<List<CardScryfall>>(stream, _jsonSerializerOptions) ?? [];
}