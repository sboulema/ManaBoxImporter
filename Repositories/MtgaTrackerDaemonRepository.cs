using ManaBoxImporter.Models.Inventory;
using ManaBoxImporter.Models.MtgaTrackerDaemon;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace ManaBoxImporter.Repositories;

public class MtgaTrackerDaemonRepository(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IInventoryRepository
{
    public async Task<InventoryModel?> GetInventory()
    {
        var client = httpClientFactory.CreateClient();
        var inventory = await client.GetFromJsonAsync<MTGATrackerDaemonInventoryImport>($"http://localhost:{configuration.GetValue<string>("Port")}/cards");

        if (inventory == null)
        {
            return new();
        }

        return new()
        {
            Cards = inventory.Cards
                .Select(card => new InventoryCard
                {
                    GroupId = card.GroupId,
                    Quantity = card.Quantity,
                })
                .ToList() ?? [],
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
        };
    }
}
