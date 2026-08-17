using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCProject.ViewModels
{
    public class ProductCreateVM
    {
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        [MaxLength(50)]
        public string SKU { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        // Display-only, populated by the controller, never posted back by the form
        [ValidateNever]
        public List<SelectListItem> Categories { get; set; } = new();

        public IFormFile ImageFile { get; set; }
    }
}
