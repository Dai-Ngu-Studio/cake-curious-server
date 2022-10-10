using Repository.Models.Product;
using System.ComponentModel.DataAnnotations;

namespace Repository.Models.Orders
{
    public class CheckoutOrder
    {
        [Required]
        public Guid? StoreId { get; set; }

        public Guid? CouponId { get; set; }

        [Required]
        public IEnumerable<CheckoutProduct>? Products { get; set; }
    }
}
