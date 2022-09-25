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
        public Task<IEnumerable<AdminDashboardStores>> GetStores(int PageSize, int PageIndex);
        public Task Update(Store obj);
        public Task Add(Store obj);
        public Task<Store> Delete(Guid id);
        public Task<Store> GetById(Guid id);
        public IEnumerable<Store> FindStore(string s, string filter_Store);
    }
}
