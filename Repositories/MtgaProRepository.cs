using ManaBoxImporter.Models.Inventory;
using ManaBoxImporter.Models.MtgaPro;
using System.Text.Json;

namespace ManaBoxImporter.Repositories;

public class MtgaProRepository : IInventoryRepository
{
    public async Task<InventoryModel?> GetInventory()
    {
        var playerLogPath = "%appdata%\\..\\LocalLow\\Wizards Of The Coast\\MTGA\\Player.log";
        playerLogPath = Environment.ExpandEnvironmentVariables(playerLogPath);
        var playerLogLines = await File.ReadAllLinesAsync(playerLogPath);
        var inventoryLine = playerLogLines.FirstOrDefault(line => line.StartsWith("[MTGA.Pro Logger] **Collection**"));

        if (string.IsNullOrEmpty(inventoryLine))
        {
            Console.WriteLine("Unable to find inventory in player log");
            return null;
        }

        var inventoryJson = inventoryLine.Replace("[MTGA.Pro Logger] **Collection**", string.Empty);
        var inventory = JsonSerializer.Deserialize<MtgaProInventoryImport>(inventoryJson, JsonSerializerOptions.Web);

        if (inventory == null)
        {
            return null;
        }

        return new()
        {
            Cards = inventory.Payload
            .Select(card => new InventoryCard
            {
                GroupId = card.Key,
                Quantity = card.Value
            })
            .ToList() ?? [],
            Timestamp = inventory.Timestamp,
        };
    }
}
