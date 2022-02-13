using System.Net.Http.Json;
using System.Text.Json;
using ManaBoxImporter.Models.Import;

Console.WriteLine("ManaBoxImporter 1.1");

if (args?.Length != 1)
{
    Console.WriteLine("Wrong number of arguments");
    Environment.Exit(-1);
}

var fileName = args.First();
var jsonString = await File.ReadAllTextAsync(fileName);
var importModel = JsonSerializer.Deserialize<ImportModel>(jsonString);

if (importModel?.Cards?.Any() != true) {
    Console.WriteLine("Unable to find any cards to be imported");
    Environment.Exit(-2);
}

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.scryfall.com/")
};

var csv = "Name,Set code,Set name,Collector number,Scryfall ID,Quantity" + Environment.NewLine;

foreach (var (card, index) in importModel.Cards.Select((value, i) => (value, i)))
{
    try
    {
        var cardScryfall = await httpClient.GetFromJsonAsync<CardScryfall>($"cards/arena/{card.GroupId}");

        if (cardScryfall == null)
        {
            continue;
        }

        // Ignore Alchemy cards
        if (cardScryfall.Name.StartsWith("A-") ||
            cardScryfall.SetCode.Equals("y22"))
        {
            continue;
        }

        Console.WriteLine($"({index + 1}/{importModel.Cards.Count}): Exporting card {cardScryfall.Name}");

        csv += $"\"{cardScryfall.Name}\",{cardScryfall.SetCode},{cardScryfall.SetName},{cardScryfall.CollectorNumber},{cardScryfall.Id},{card.Quantity}" + Environment.NewLine;
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        Console.WriteLine($"Error exporting card {card.GroupId}");
    }

    Thread.Sleep(50);
}

var exportFilePath = Path.ChangeExtension(fileName, "csv");

await File.WriteAllTextAsync(exportFilePath, csv);

Console.WriteLine($"Export completed!");
Console.WriteLine($"CSV available at {exportFilePath}");