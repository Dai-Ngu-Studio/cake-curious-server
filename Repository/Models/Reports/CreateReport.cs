using System.ComponentModel.DataAnnotations;

namespace Repository.Models.Reports
{
    public class CreateReport
    {
        [Required]
        public int? ReportCategoryId { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }

        [Required]
        public Guid? ItemId { get; set; }

        [Required]
        public int? ItemType { get; set; }
    }
}
