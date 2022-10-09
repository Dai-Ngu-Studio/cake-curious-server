using System.ComponentModel.DataAnnotations;

namespace Repository.Models.Product
{
    public class CheckoutProduct
    {
        [Required]
        public Guid? Id { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int? Quantity { get; set; }
    }
}
