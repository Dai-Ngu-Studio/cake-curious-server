using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Models.Recipes;
using Repository.Models.Users;

namespace Repository
{
    public class RecipeRepository : IRecipeRepository
    {
        public async Task Add(Recipe obj)
        {
            var db = new CakeCuriousDbContext();
            db.Recipes.Add(obj);
            await db.SaveChangesAsync();
        }

        // Recipe was published within 2 days
        // Recipe has an author which is followed by the follower with UID
        public ICollection<FollowRecipe> GetLatestRecipesForFollower(string uid, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Recipes
                .OrderBy(x => x.Id)
                .Where(x => x.PublishedDate!.Value <= DateTime.Now
                && x.PublishedDate!.Value >= DateTime.Now.AddDays(-2))
                .Where(x => x.User!.Followers!.Any(x => x.FollowerId == uid))
                .Skip(skip)
                .Take(take)
                .AsSplitQuery()
                .Select(x => new FollowRecipe
                {
                    Id = x.Id,
                    User = new SimpleUser
                    {
                        Id = x.UserId,
                        DisplayName = x.User!.DisplayName,
                        PhotoUrl = x.User.PhotoUrl,
                    },
                    Name = x.Name,
                    ServingSize = x.ServingSize,
                    PhotoUrl = x.PhotoUrl,
                    CookTime = x.CookTime,
                    Likes = x.Likes!.Count,
                })
                .ToList();
        }
    }
}
