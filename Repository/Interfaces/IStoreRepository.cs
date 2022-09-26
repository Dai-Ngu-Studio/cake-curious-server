using BusinessObject;
using Repository.Models.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IStoreRepository
    {
        public Task Update(Store obj);
        public Task Add(Store obj);
        public Task<Store?> Delete(Guid? id);
        public Task<Store?> GetById(Guid id);
        public IEnumerable<AdminDashboardStore>? GetStores(string? s, string? filter_Store, int PageSize, int PageIndex);
        public int CountDashboardStores(string? s,string? filter_Store);
    }
}
