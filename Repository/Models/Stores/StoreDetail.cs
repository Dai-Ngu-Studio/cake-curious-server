using Repository.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Stores
{
    public class StoreDetail
    {
        public Guid? Id { get; set; }

        public string? UserId { get; set; }

        public StoreDetailUser? User { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? PhotoUrl { get; set; }

        public string? Address { get; set; }

        public decimal? Rating { get; set; }

        public int? Status { get; set; }
    }
}
