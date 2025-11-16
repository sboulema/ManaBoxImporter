using CsvHelper;
using ManaBoxImporter.Models.Inventory;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace ManaBoxImporter.Services;

public class OutputService(IConfiguration configuration)
{
    public async Task<string> Output(List<InventoryCard> inventory, string timestamp)
    {
        var outputFilePath = GetOutputFilePath(timestamp);

        using var writer = new StreamWriter(outputFilePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(inventory);

        return outputFilePath;
    }

    private string GetOutputFilePath(string timestamp)
    {
        var outputDirectory = !string.IsNullOrEmpty(configuration.GetValue<string>("OutputFilePath"))
            ? Path.GetDirectoryName(configuration.GetValue<string>("OutputFilePath")!.Trim())
            : Path.GetTempPath();

        return Path.Combine(outputDirectory!, $"collection-{timestamp}.csv");
    }
}
