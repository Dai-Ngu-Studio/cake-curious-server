using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("Measurement")]
    public class Measurement
    {
        [Key]
        [Column("measurement_unit", TypeName = "nvarchar(24)")]
        public string? MeasurementUnit { get; set; }
    }
}
