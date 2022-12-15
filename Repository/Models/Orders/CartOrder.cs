using Repository.Models.Coupons;
using Repository.Models.Product;
using Repository.Models.Stores;

namespace Repository.Models.Orders
{
    public class CartOrder
    {
        public CartStore? Store { get; set; }
        public IEnumerable<CartDetailProduct>? Products { get; set; }
        public UserAwareSimpleCoupon? Coupon { get; set; }
    }
}
