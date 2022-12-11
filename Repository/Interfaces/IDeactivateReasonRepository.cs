using BusinessObject;
using Repository.Models.DeactivateReasons;

namespace Repository.Interfaces
{
    public interface IDeactivateReasonRepository
    {
        public Task CreateReason(DeactivateReason reason);
        public Task<DetailDeactivateReason?> GetReasonByItemIdReadonly(Guid itemId);
    }
}
