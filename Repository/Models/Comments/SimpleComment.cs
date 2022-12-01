using Repository.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Comments
{
    public class SimpleComment
    {
        public Guid? Id { get; set; }
        public SimpleUser? User { get; set; }
        public string? Content { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public IEnumerable<RecipeCommentMedia>? Images { get; set; }
    }
}
