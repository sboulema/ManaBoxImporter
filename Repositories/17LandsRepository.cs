using CsvHelper;
using ManaBoxImporter.Models.SeventeenLands;
using System.Globalization;

namespace ManaBoxImporter.Repositories;

public class SeventeenLandsRepository(IHttpClientFactory httpClientFactory)
{
    public async Task<IEnumerable<SeventeenLandsCard>> Get17LandsCards()
    {
        Console.WriteLine("Downloading 17Lands database");

        var client = httpClientFactory.CreateClient();
        var stream = await client.GetStreamAsync("https://17lands-public.s3.amazonaws.com/analysis_data/cards/cards.csv");
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return [.. csv.GetRecords<SeventeenLandsCard>()];
    }
}
