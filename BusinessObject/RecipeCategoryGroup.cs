﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("RecipeCategoryGroup")]
    public class RecipeCategoryGroup
    {
        public RecipeCategoryGroup()
        {
            RecipeCategories = new HashSet<RecipeCategory>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public int? Id { get; set; }

        [Column("name", TypeName = "nvarchar(32)")]
        public string? Name { get; set; }

        [InverseProperty("RecipeCategoryGroup")]
        public ICollection<RecipeCategory>? RecipeCategories { get; set; }
    }
}