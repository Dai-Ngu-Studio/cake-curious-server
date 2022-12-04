using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("DeactivateReason")]
    public class DeactivateReason
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("item_id")]
        public Guid? ItemId { get; set; }

        [Column("item_type")]
        public int? ItemType { get; set; }

        [Column("staff_id", TypeName = "varchar(128)")]
        public string? StaffId { get; set; }

        [Column("reason", TypeName = "nvarchar(512)")]
        public string? Reason { get; set; }

        [Column("deactivate_date", TypeName = "datetime2(7)")]
        public DateTime? DeactivateDate { get; set; }
    }
}
