using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("ViolationReport")]
    public class ViolationReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("title", TypeName = "nvarchar(256)")]
        public string? Title { get; set; }

        [Column("content", TypeName = "nvarchar(512)")]
        public string? Content { get; set; }

        [Column("reporter_id", TypeName = "varchar(128)")]
        public string? ReporterId { get; set; }

        [ForeignKey("ReporterId")]
        public User? Reporter { get; set; }

        [Column("staff_id", TypeName = "varchar(128)")]
        public string? StaffId { get; set; }

        [ForeignKey("StaffId")]
        public User? Staff { get; set; }

        [Column("item_type")]
        public int ItemType { get; set; }

        [Column("submitted_date", TypeName = "datetime2(7)")]
        public DateTime? SubmittedDate { get; set; }

        [Column("status")]
        public int Status { get; set; }
    }
}
