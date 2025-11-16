using ManaBoxImporter.Models.Inventory;

namespace ManaBoxImporter.Repositories;

public interface IInventoryRepository
{
    Task<InventoryModel?> GetInventory();
}
