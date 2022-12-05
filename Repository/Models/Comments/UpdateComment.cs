using System.ComponentModel.DataAnnotations;

namespace Repository.Models.Comments
{
    public class UpdateComment
    {
        [Required]
        public string? Content { get; set; }
    }
}
