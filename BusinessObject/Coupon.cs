﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("Coupon")]
    public class Coupon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("store_id")]
        public Guid? StoreId { get; set; }

        [ForeignKey("StoreId")]
        public Store? Store { get; set; }

        [Column("name", TypeName = "nvarchar(256)")]
        public string? Name { get; set; }

        [Column("name", TypeName = "varchar(24)")]
        public string? Code { get; set; }

        [Column("max_uses")]
        public int MaxUses { get; set; }

        [Column("used_count")]
        public int UsedCount { get; set; }

        [Column("expiry_date", TypeName = "datetime2(7)")]
        public DateTime? ExpiryDate { get; set; }

        [Column("status")]
        public int Status { get; set; }
    }
}
