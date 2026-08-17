using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MVCProject.Models.Base;
using MVCProject.Utilities.Enums;

namespace MVCProject.Models
{
    public class Order : BaseEntity
    {
        public string UserId { get; set; }

        [ValidateNever]
        public AppUser User { get; set; }

        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [ValidateNever]
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
