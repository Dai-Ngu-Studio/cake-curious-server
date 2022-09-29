using BusinessObject;
using Repository.Models.ReportCategories;
using Repository.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class StaffDashboardReport
    {
        public Guid Id { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public Guid? ItemId { get; set; }
        public int? ItemType { get; set; }
        public SimpleUser? Reporter { get; set; }
        public string? Title { get; set; }
        public SimpleUser? Staff { get; set; }   
        public string? StaffId { get; set; }
        public SimpleUser? ReportedUser { get; set; }
        public int Status { get; set; }
    }
}
