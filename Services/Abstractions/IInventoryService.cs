using MyProject.Models.Entities;

namespace MyProject.Services.Abstractions
{
    public interface IInventoryService
    {
        Task<List<Inventory>> GetInventory(int userId);
    }
}
