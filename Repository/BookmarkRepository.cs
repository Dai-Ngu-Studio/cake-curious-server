using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

namespace Repository
{
    public class BookmarkRepository : IBookmarkRepository
    {
        public async Task<bool> IsRecipeBookmarkedByUser(string userId, Guid recipeId)
        {
            var db = new CakeCuriousDbContext();
            return await db.Bookmarks.AnyAsync(x => x.UserId == userId && x.RecipeId == recipeId);
        }

        public async Task Add(string userId, Guid recipeId)
        {
            var db = new CakeCuriousDbContext();
            await db.Bookmarks.AddAsync(new Bookmark
            {
                UserId = userId,
                RecipeId = recipeId,
                CreatedDate = DateTime.Now,
            });
            await db.SaveChangesAsync();
        }

        public async Task Remove(string userId, Guid recipeId)
        {
            var db = new CakeCuriousDbContext();
            var bookmark = await db.Bookmarks.FirstOrDefaultAsync(x => x.UserId == userId && x.RecipeId == recipeId);
            db.Bookmarks.Remove(bookmark!);
            await db.SaveChangesAsync();
        }
    }
}
