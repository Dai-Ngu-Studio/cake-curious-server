using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("ReportCategory")]
    public class ReportCategory
    {
        public ReportCategory()
        {
            ViolationReports = new HashSet<ViolationReport>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public int? Id { get; set; }

        [Column("name", TypeName = "nvarchar(48)")]
        public string? Name { get; set; }

        [Column("en_name", TypeName = "nvarchar(48)")]
        public string? EnName { get; set; }

        [InverseProperty("ReportCategory")]
        public ICollection<ViolationReport>? ViolationReports { get; set; }
    }
}
