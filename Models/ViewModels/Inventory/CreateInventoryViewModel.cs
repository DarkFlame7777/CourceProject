using System.ComponentModel.DataAnnotations;

namespace MyProject.Models.ViewModels.Inventory
{
    public class CreateInventoryViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Fields { get; set; } = "[]";
    }
}
