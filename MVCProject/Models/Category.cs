using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MVCProject.Models.Base;

namespace MVCProject.Models
{
    public class Category : BaseEntity
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Image is required")]
        public string ImageUrl { get; set; }

        [ValidateNever]
        public List<Product> Products { get; set; }
    }
}
