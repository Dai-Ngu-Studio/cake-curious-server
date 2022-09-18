using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("Color")]
    public class Color
    {
        [Key]
        [Column("hex_code", TypeName = "varchar(8)")]
        public string? HexCode { get; set; }
    }
}
