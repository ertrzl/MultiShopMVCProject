using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MVCProject.Models.Base;

namespace MVCProject.Models
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }

        [ValidateNever]
        public Order Order { get; set; }

        public int ProductId { get; set; }

        [ValidateNever]
        public Product Product { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}
