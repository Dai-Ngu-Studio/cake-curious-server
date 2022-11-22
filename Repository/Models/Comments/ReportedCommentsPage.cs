using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Comments
{
    public class ReportedCommentsPage
    {
        public IEnumerable<SimpleCommentForReportList>? Comments { get; set; }
        public int? TotalPage { get; set; }
    }
}
