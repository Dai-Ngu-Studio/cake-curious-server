using System.ComponentModel.DataAnnotations;

namespace Repository.Models.OrderDetails
{
    public class RateOrderDetail
    {
        [Required]
        public decimal? Rating { get; set; }
    }
}
