namespace Repository.Models.DeactivateReasons
{
    public class CreateDeactivateReason
    {
        public Guid? ItemId { get; set; }
        public int? ItemType { get; set; }
        public string? Reason { get; set; }
        public string? UserId { get; set; }
    }
}
