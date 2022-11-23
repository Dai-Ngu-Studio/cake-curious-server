using BusinessObject;
using Repository.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Comments
{
    public class SimpleCommentForReportList
    {
        public Guid? Id { get; set; }
        public string? UserId { get; set; }
        public StoreDetailUser? User { get; set; }
        public string? Content { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public int? Status { get; set; }
        public int? TotalPendingReport { get; set; }
        public ICollection<SimpleCommentMedia>? Images { get; set; }

    }
}
