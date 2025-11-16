using ManaBoxImporter.Models.Inventory;
using ManaBoxImporter.Repositories;
using Microsoft.Extensions.Configuration;

namespace ManaBoxImporter.Services;

public class InventoryService(
    MtgaTrackerDaemonRepository mtgaTrackerDaemonRepository,
    MtgaProRepository mtgaProRepository,
    IConfiguration configuration)
{
    public async Task<InventoryModel?> GetInventory()
    {
        InventoryModel? inventory;

        if (!string.IsNullOrEmpty(configuration.GetValue<string>("port")))
        {
            Console.WriteLine("Using MTGA Tracker Daemon to import your inventory");
            inventory = await mtgaTrackerDaemonRepository.GetInventory();
        }
        else
        {
            Console.WriteLine("Using MTGA Pro to import your inventory");
            inventory = await mtgaProRepository.GetInventory();
        }

        if (inventory?.Cards.Any() == true)
        {
            Console.WriteLine($"Imported {inventory.Cards.Count} cards from your inventory");
        }

        return inventory;
    }
}
