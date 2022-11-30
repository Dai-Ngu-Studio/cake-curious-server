using Repository.Models.Product;
using Repository.Models.Stores;

namespace Repository.Models.Orders
{
    public class BundleOrder
    {
        public BundleStore? Store { get; set; }
        public IEnumerable<BundleDetailProduct>? Products { get; set; }
    }
}
