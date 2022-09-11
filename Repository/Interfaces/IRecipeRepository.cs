using BusinessObject;

namespace Repository.Interfaces
{
    public interface IRecipeRepository
    {
        public IQueryable<Recipe> GetList();
        public Task Add(Recipe obj);
    }
}
