namespace Repository.Models.Orders
{
    public class InfoOrderPage
    {
        public int? TotalPages { get; set; }
        public IEnumerable<InfoOrder>? Orders { get; set; }
    }
}
