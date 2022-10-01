﻿using BusinessObject;
using Repository.Models;
using Repository.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IViolationReportRepository
    {
        public Task<ItemReportContent?> GetReportedItemDetail(Guid? itemId);
        public Task Update(ViolationReport obj);
        public Task Add(ViolationReport obj);
        public Task<ViolationReport?> GetById(Guid id);
        public Task<IEnumerable<StaffDashboardReport>?> GetViolationReports(string? s,string? order_by ,string? filter_ViolationReport, int PageSize, int PageIndex);
        public int CountDashboardViolationReports(string? s, string? order_by, string? report_type);
    }
}
