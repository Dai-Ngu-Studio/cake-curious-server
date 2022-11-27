using Repository.Models.Coupons;
using Repository.Models.Stores;

namespace Repository.Models.Product
{
    public class CartOrder
    {
        public CartStore? Store { get; set; }
        public IEnumerable<CartDetailProduct>? Products { get; set; }
        public SimpleCoupon? Coupon { get; set; }
    }
}
