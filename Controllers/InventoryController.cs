using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models.Entities;
using MyProject.Models.ViewModels.Inventory;
using MyProject.Services.Abstractions;
using System.Security.Claims;
using System.Text.Json;

namespace MyProject.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _dbContext; // Delete
        private readonly IInventoryService _inventoryService;

        public InventoryController(ApplicationDbContext dbContext, IInventoryService inventoryContext)
        {
            _dbContext = dbContext; // Delete
            _inventoryService = inventoryContext;
        }

        [HttpGet]
        public async Task<IActionResult> MyInventories()
        {
            var userId = GetUserId();

            var inventories = await _inventoryService.GetInventory(userId);

            return View(inventories);
        }

        private int GetUserId()
            => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpGet]
        public IActionResult Create()
            => View(new CreateInventoryViewModel());

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();

            var inventory = await GetInventoryAsync(id, userId);

            if (inventory == null) return NotFound();

            var model = CreateModel(inventory);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            //InventoryService
            var inventory = await _dbContext.Inventories
                .Include(i => i.Creator)
                .FirstOrDefaultAsync(i => i.Id == id);
            //

            if (inventory == null) return NotFound();

            //InventoryService
            var items = await _dbContext.Items
                .Include(i => i.CreatedBy)
                .Where(i => i.InventoryId == id)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
            //

            SetData(inventory, items);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInventoryViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = GetUserId();

            var fields = string.IsNullOrEmpty(model.Fields) ? "[]" : model.Fields;

            //InventoryService
            var inventory = CreateInventory(model.Title, userId, fields);

            _dbContext.Inventories.Add(inventory);
            await _dbContext.SaveChangesAsync();
            //

            return RedirectToAction("MyInventories");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateInventoryViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = GetUserId();

            var inventory = await GetInventoryAsync(id, userId);

            if (inventory == null) return NotFound();

            SetInform(model, inventory);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("MyInventories");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(List<int> selectedIds)
        {
            if (selectedIds != null || selectedIds.Any())
            {
                var userId = GetUserId();

                //InventoryService
                var inventories = await _dbContext.Inventories
                    .Where(i => selectedIds.Contains(i.Id) && i.CreatorId == userId)
                    .ToListAsync();

                _dbContext.Inventories.RemoveRange(inventories);
                await _dbContext.SaveChangesAsync();
                //
            }

            return RedirectToAction("MyInventories");
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(int inventoryId, List<string> values)
        {
            var userId = GetUserId();

            //InventoryService
            var item = CreateItem(inventoryId, userId, values);

            _dbContext.Items.Add(item);
            await _dbContext.SaveChangesAsync();
            //

            return RedirectToAction("Details", new { id = inventoryId });
        }

        //InventoryService
        private Inventory CreateInventory(string title, int userId, string fields)
            => new Inventory
            {
                Title = title,
                CreatorId = userId,
                FieldsJson = fields
            };
        //

        private CreateInventoryViewModel CreateModel(Inventory inventory)
            => new CreateInventoryViewModel
            {
                Title = inventory.Title,
                Fields = inventory.FieldsJson
            };


        //InventoryService
        private async Task<Inventory> GetInventoryAsync(int id, int userId)
            => await _dbContext.Inventories
                .FirstOrDefaultAsync(i => i.Id == id && i.CreatorId == userId);
        //

        private void SetInform(CreateInventoryViewModel model, Inventory inventory)
        {
            inventory.Title = model.Title;
            inventory.FieldsJson = model.Fields;
        }

        private void SetData(Inventory inventory, List<Item> items)
        {
            ViewBag.Inventory = inventory;
            ViewBag.Items = items;
            ViewBag.Fields = JsonSerializer.Deserialize<List<InventoryField>>(inventory.FieldsJson) ?? new();
        }

        //InventoryService
        private Item CreateItem(int inventoryId, int userId, List<string> values)
            => new Item
            {
                InventoryId = inventoryId,
                CreatedById = userId,
                ValuesJson = JsonSerializer.Serialize(values)
            };
        //
    }
}
