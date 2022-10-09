using System.ComponentModel.DataAnnotations;

namespace Repository.Models.Orders
{
    public class CheckoutOrders
    {
        [Required]
        public IEnumerable<CheckoutOrder>? Orders { get; set; }

        [Required]
        public string? Address { get; set; }
    }
}
