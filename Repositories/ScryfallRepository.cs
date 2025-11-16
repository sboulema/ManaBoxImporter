using ManaBoxImporter.Models.Scryfall;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace ManaBoxImporter.Repositories;

public class ScryfallRepository(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration)
{
    public async Task<IEnumerable<ScryfallCard>> GetCards()
    {
        var bulkDataLocation = await GetBulkDataLocation();

        return GetScryfallCardsFromFile(bulkDataLocation);
    }

    private static IEnumerable<ScryfallCard> GetScryfallCardsFromFile(string path)
    {
        Console.WriteLine("Loading Scryfall database from file");
        using FileStream stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<IEnumerable<ScryfallCard>>(stream, JsonSerializerOptions.Web) ?? [];
    }

    private async Task DownloadScryfallCardsToFile(string downloadUri, string path)
    {
        Console.WriteLine("Downloading Scryfall database to file, this may take a while...");
        var client = httpClientFactory.CreateClient("Scryfall");
        using var stream = await client.GetStreamAsync(downloadUri);
        using var fileStream = new FileStream(path, FileMode.OpenOrCreate);
        await stream.CopyToAsync(fileStream);
    }

    private async Task<ScryfallBulkData?> GetBulkDataDefaultCards()
    {
        var bulkData = await GetBulkData();
        return bulkData?.Data.FirstOrDefault(d => d.Type == "default_cards");
    }

    private async Task<ScryfallBulkDataResponse?> GetBulkData()
    {
        var client = httpClientFactory.CreateClient("Scryfall");
        return await client.GetFromJsonAsync<ScryfallBulkDataResponse>("https://api.scryfall.com/bulk-data");
    }

    private async Task<string> GetBulkDataLocation()
    {
        var argPath = configuration.GetValue<string>("ScryfallFilePath");

        if (File.Exists(argPath))
        {
            return argPath;
        }

        var bulkData = await GetBulkDataDefaultCards();
        var bulkdataFileName = $"default-cards-{bulkData?.UpdatedAt:yyyyMMddHHmmss}.json";
        var bulkDataLocation = Path.Combine(Path.GetTempPath(), bulkdataFileName);

        if (!File.Exists(bulkDataLocation))
        {
            await DownloadScryfallCardsToFile(bulkData!.DownloadUri, bulkDataLocation);
        }

        return bulkDataLocation;
    }
}
