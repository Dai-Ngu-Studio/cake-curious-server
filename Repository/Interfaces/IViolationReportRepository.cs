using BusinessObject;
using Repository.Models.Reports;

namespace Repository.Interfaces
{
    public interface IViolationReportRepository
    {
        public Task<ItemReportContent?> GetReportedItemDetail(Guid? itemId);
        public Task Update(ViolationReport obj);
        public Task Add(ViolationReport obj);
        public Task<StaffReportDetail?> GetReportDetailById(Guid id);
        public Task<ViolationReport?> GetById(Guid id);
        public Task<string?> BulkUpdate(Guid[] guid, string uid);
        public Task UpdateAllReportStatusOfAnItem(Guid itemId, string uid);
        public Task<IEnumerable<StaffDashboardReport>?> GetReportsOfAnItem(Guid itemId, string? s, string? order_by, string? filter, int PageSize, int PageIndex);
        public int CountDashboardViolationReportsOfAnItem(Guid itemId, string? s, string? filter);
        public Task<int> CountPendingReportOfAnItem(Guid itemId);
        public IEnumerable<string> GetReportersOfAnItemReadonly(Guid itemId, int skip, int take);
    }
}
