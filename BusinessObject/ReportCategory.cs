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

        [Column("name", TypeName = "nvarchar(32)")]
        public string? Name { get; set; }

        [Column("lang_code", TypeName = "varchar(128)")]
        public string? LangCode { get; set; }

        [InverseProperty("ReportCategory")]
        public ICollection<ViolationReport>? ViolationReports { get; set; }
    }
}
