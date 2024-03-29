﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("RecipeCategory")]
    public class RecipeCategory
    {
        public RecipeCategory()
        {
            HasRecipes = new HashSet<RecipeHasCategory>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public int? Id { get; set; }

        [Column("name", TypeName = "nvarchar(48)")]
        public string? Name { get; set; }

        [Column("en_name", TypeName = "nvarchar(48)")]
        public string? EnName { get; set; }

        [Column("group_id")]
        public int? RecipeCategoryGroupId { get; set; }

        [ForeignKey("RecipeCategoryGroupId")]
        public RecipeCategoryGroup? RecipeCategoryGroup { get; set; }

        [InverseProperty("RecipeCategory")]
        public ICollection<RecipeHasCategory>? HasRecipes { get; set; }
    }
}
