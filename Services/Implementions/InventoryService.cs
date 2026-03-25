using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models.Entities;
using MyProject.Services.Abstractions;

namespace MyProject.Services.Implementions
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _dbContext;
        public InventoryService(ApplicationDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public async Task<List<Inventory>> GetInventory(int userId)
            => await _dbContext.Inventories
                .Where(i => i.CreatorId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();


    }
}
