﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.DashboardReports
{
    public class AdminDashboardBarChart
    {
        public List<int>? CurrentMonthReport { get; set; } = new List<int>() { 0, 0, 0, 0 };
        public List<int>? LastMonthReport { get; set; } = new List<int>() { 0, 0, 0, 0 };
        public List<string> Week { get; set; } = new List<string> { "Tuần 1", "Tuần 2", "Tuần 3", "Tuần 4" };

    }
}
