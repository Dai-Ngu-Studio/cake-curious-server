using BusinessObject;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Recipes;
using Repository.Constants.Reports;
using Repository.Constants.Users;
using Repository.Interfaces;
using Repository.Models.Recipes;
using Repository.Models.RecipeSteps;

namespace Repository
{
    public class RecipeRepository : IRecipeRepository
    {
        public async Task<Recipe?> GetRecipe(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Recipes.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Recipe?> GetRecipeWithStepsReadonly(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Recipes.AsNoTracking().Include(x => x.RecipeSteps).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Recipe?> GetRecipeReadonly(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Recipes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<int> UpdateShareUrl(Guid id, string shareUrl)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                string query = "update [Recipe] set [Recipe].[share_url] = {0} where [Recipe].[id] = {1}";
                var rows = await db.Database.ExecuteSqlRawAsync(query, shareUrl, id);
                await transaction.CommitAsync();
                return rows;
            }
        }

        public async Task<int> Delete(Guid id)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                string query = "update [Recipe] set [Recipe].[status] = {0} where [Recipe].[id] = {1}";
                var rows = await db.Database.ExecuteSqlRawAsync(query, (int)RecipeStatusEnum.Inactive, id);
                await transaction.CommitAsync();
                return rows;
            }
        }

        public async Task<ICollection<ExploreRecipe>> Explore(int randSeed, int take, int key)
        {
            var result = new List<ExploreRecipe>();
            var db = new CakeCuriousDbContext();
            string query = $"select top {take} [r].id, [R].[name], [r].[photo_url], abs(checksum([r].[id], rand(@randSeed)*rand(@randSeed))) as [key] from [Recipe] as [r] left join [User] as [u] on [r].[user_id] = [u].[id] where abs(checksum([r].[id], rand(@randSeed)*rand(@randSeed))) > @key and [r].[status] = @recipeStatus and [u].[status] = @userStatus order by abs(checksum([r].[id], rand(@randSeed)*rand(@randSeed)))";
            var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = query;
            var recipeStatus = (int)RecipeStatusEnum.Active;
            var userStatus = (int)UserStatusEnum.Active;
            cmd.Parameters.Add(new SqlParameter("@randSeed", randSeed));
            cmd.Parameters.Add(new SqlParameter("@key", key));
            cmd.Parameters.Add(new SqlParameter("@recipeStatus", recipeStatus));
            cmd.Parameters.Add(new SqlParameter("@userStatus", userStatus));
            if (cmd.Connection!.State != System.Data.ConnectionState.Open)
            {
                await cmd.Connection.OpenAsync();
            }
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    result.Add(new ExploreRecipe
                    {
                        Id = (Guid)reader["id"],
                        Name = (string)reader["name"],
                        PhotoUrl = (string)reader["photo_url"],
                        Key = (int)reader["key"],
                    });
                }
            }
            if (cmd.Connection!.State == System.Data.ConnectionState.Open)
            {
                await cmd.Connection!.CloseAsync();
            }
            return result;
        }

        public async Task<EditRecipe?> GetEditRecipe(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Recipes
                .Where(x => x.Id == id)
                .ProjectToType<EditRecipe>()
                .FirstOrDefaultAsync();
        }

        public async Task UpdateRecipe(Recipe recipe, Recipe updateRecipe)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                // Set new information
                recipe.Name = updateRecipe.Name;
                recipe.Description = updateRecipe.Description;
                recipe.ServingSize = updateRecipe.ServingSize;
                recipe.PhotoUrl = updateRecipe.PhotoUrl;
                recipe.ThumbnailUrl = updateRecipe.ThumbnailUrl;
                recipe.CookTime = updateRecipe.CookTime;

                db.Recipes.Update(recipe);

                // Remove old step materials
                string query = $"delete from [RecipeStepMaterial] where [RecipeStepMaterial].material_id in (select [RecipeMaterial].id from [RecipeMaterial] where [RecipeMaterial].recipe_id = '{recipe.Id}')";
                await db.Database.ExecuteSqlRawAsync(query);

                // Remove old steps/materials/categories/media
                query = $"delete from [RecipeStep] where [RecipeStep].recipe_id = '{recipe.Id}'  delete from [RecipeMaterial] where [RecipeMaterial].recipe_id = '{recipe.Id}'  delete from [RecipeHasCategory] where [RecipeHasCategory].recipe_id = '{recipe.Id}'  delete from [RecipeMedia] where [RecipeMedia].recipe_id = '{recipe.Id}'";
                await db.Database.ExecuteSqlRawAsync(query);

                // Set new steps/materials/categories/media
                foreach (var step in updateRecipe.RecipeSteps!)
                {
                    step.RecipeId = recipe.Id;
                }

                foreach (var material in updateRecipe.RecipeMaterials!)
                {
                    material.RecipeId = recipe.Id;
                }

                foreach (var category in updateRecipe.HasCategories!)
                {
                    category.RecipeId = recipe.Id;
                }

                foreach (var media in updateRecipe.RecipeMedia!)
                {
                    media.RecipeId = recipe.Id;
                }

                await db.RecipeSteps.AddRangeAsync(updateRecipe.RecipeSteps!);
                await db.RecipeMaterials.AddRangeAsync(updateRecipe.RecipeMaterials!);
                await db.RecipeHasCategories.AddRangeAsync(updateRecipe.HasCategories!);
                await db.RecipeMedia.AddRangeAsync(updateRecipe.RecipeMedia!);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
        }

        public async Task AddRecipe(Recipe recipe)
        {
            var db = new CakeCuriousDbContext();
            await db.Recipes.AddAsync(recipe);
            await db.SaveChangesAsync();
        }

        public async Task<DetailRecipeStep?> GetRecipeStepDetails(Guid recipeId, int stepNumber)
        {
            var db = new CakeCuriousDbContext();
            return await db.RecipeSteps
                .AsNoTracking()
                .Where(x => x.RecipeId == recipeId && x.StepNumber == stepNumber)
                .ProjectToType<DetailRecipeStep>()
                .FirstOrDefaultAsync();
        }

        public async Task<DetailRecipe?> GetRecipeDetails(Guid recipeId, string userId)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("userId", userId);

                var db = new CakeCuriousDbContext();
                return await db.Recipes
                    .AsNoTracking()
                    .Where(x => x.Id == recipeId)
                    .ProjectToType<DetailRecipe>()
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<int> CountLatestRecipesForFollower(string uid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Recipes
                .AsNoTracking()
                .Where(x => x.User!.Followers!.Any(x => x.FollowerId == uid))
                .Where(x => x.Status == (int)RecipeStatusEnum.Active)
                .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                .CountAsync();
        }

        // Recipe was published within 2 days
        // Recipe has an author which is followed by the follower with UID
        public IEnumerable<HomeRecipe> GetLatestRecipesForFollower(string uid, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Recipes
                .AsNoTracking()
                .OrderByDescending(x => x.PublishedDate)
                .Where(x => x.User!.Followers!.Any(x => x.FollowerId == uid))
                .Where(x => x.Status == (int)RecipeStatusEnum.Active)
                .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                .Skip(skip)
                .Take(take)
                .ProjectToType<HomeRecipe>();
        }

        public async Task<int> CountTrendingRecipes(int period)
        {
            var db = new CakeCuriousDbContext();
            return await db.Recipes
                .AsNoTracking()
                .Where(x => x.PublishedDate!.Value <= DateTime.Now.AddDays(3 * (-period)))
                .Where(x => x.PublishedDate!.Value > DateTime.Now.AddDays(-3 * (period + 1)))
                .Where(x => x.Status == (int)RecipeStatusEnum.Active)
                .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                .CountAsync();
        }

        public IEnumerable<HomeRecipe> GetTrendingRecipes(int period, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Recipes
                .AsNoTracking()
                .OrderByDescending(x => x.Likes!.Count)
                .Where(x => x.PublishedDate!.Value <= DateTime.Now.AddDays(3 * (-period)))
                .Where(x => x.PublishedDate!.Value > DateTime.Now.AddDays(-3 * (period + 1)))
                .Where(x => x.Status == (int)RecipeStatusEnum.Active)
                .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                .Skip(skip)
                .Take(take)
                .ProjectToType<HomeRecipe>();
        }

        public async Task<int> CountBookmarksOfUser(string userId)
        {
            var db = new CakeCuriousDbContext();
            return await db.Bookmarks
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Where(x => x.Recipe!.Status == (int)RecipeStatusEnum.Active)
                .CountAsync();
        }

        public IEnumerable<HomeRecipe> GetBookmarksOfUser(string userId, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Bookmarks
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedDate)
                .Where(x => x.UserId == userId)
                .Where(x => x.Recipe!.Status == (int)RecipeStatusEnum.Active)
                .Skip(skip)
                .Take(take)
                .ProjectToType<HomeRecipe>();
        }

        public async Task<int> CountLikedOfUser(string userId)
        {
            var db = new CakeCuriousDbContext();
            return await db.Likes
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Where(x => x.Recipe!.Status == (int)RecipeStatusEnum.Active)
                .CountAsync();
        }

        public IEnumerable<HomeRecipe> GetLikedOfUser(string userId, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Likes
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedDate)
                .Where(x => x.UserId == userId)
                .Where(x => x.Recipe!.Status == (int)RecipeStatusEnum.Active)
                .Skip(skip)
                .Take(take)
                .ProjectToType<HomeRecipe>();
        }

        public async Task<int> CountRecipesOfUser(string userId)
        {
            var db = new CakeCuriousDbContext();
            return await db.Recipes
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Where(x => x.Status == (int)RecipeStatusEnum.Active)
                .CountAsync();
        }

        public IEnumerable<HomeRecipe> GetRecipesOfUser(string userId, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Recipes
                .AsNoTracking()
                .OrderByDescending(x => x.Likes!.Count)
                .Where(x => x.UserId == userId)
                .Where(x => x.Status == (int)RecipeStatusEnum.Active)
                .Skip(skip)
                .Take(take)
                .ProjectToType<HomeRecipe>();
        }

        public async Task<ICollection<HomeRecipe>> GetSuggestedRecipes(List<Guid> recipeIds)
        {
            var db = new CakeCuriousDbContext();
            return await db.Recipes
                .AsNoTracking()
                .Where(x => recipeIds.Any(y => y == (Guid)x.Id!))
                .Where(x => x.Status == (int)RecipeStatusEnum.Active)
                .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                .ProjectToType<HomeRecipe>()
                .ToListAsync();
        }
        public List<SimpleRecipeForReportList>? OrderByAscName(List<SimpleRecipeForReportList>? isReportedRecipes)
        {
            return isReportedRecipes!.OrderBy(p => p.Name).ToList();
        }
        public List<SimpleRecipeForReportList>? OrderByDescName(List<SimpleRecipeForReportList>? isReportedRecipes)
        {
            return isReportedRecipes!.OrderByDescending(p => p.Name).ToList();
        }
        public List<SimpleRecipeForReportList>? FilterByStatusActiveList(List<SimpleRecipeForReportList>? isReportedRecipes)
        {
            return isReportedRecipes!.Where(p => p.Status == (int)RecipeStatusEnum.Active).ToList();
        }

        public List<SimpleRecipeForReportList>? FilterByStatusInactive(List<SimpleRecipeForReportList>? isReportedRecipes)
        {
            return isReportedRecipes!.Where(p => p.Status == (int)RecipeStatusEnum.Inactive).ToList();
        }

        public List<SimpleRecipeForReportList>? SearchRecipes(string? keyWord, List<SimpleRecipeForReportList>? reportsRecipeType)
        {

            return reportsRecipeType!.Where(p => p.Name!.Contains(keyWord!)).ToList();
        }
        public async Task<IEnumerable<SimpleRecipeForReportList>> GetReportedRecipes(string? s, string? sort, string? filter, int page, int size)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<ViolationReport> reportsRecipeType = await db.ViolationReports.Where(report => report.ItemType == (int)ReportTypeEnum.Recipe).GroupBy(x => x.ItemId).Select(d => d.First()).ToListAsync();
            List<SimpleRecipeForReportList>? isReportedRecipes = new List<SimpleRecipeForReportList>();
            if (reportsRecipeType.Count() > 0)
                foreach (var report in reportsRecipeType)
                {
                    SimpleRecipeForReportList recipe = (await db.Recipes.ProjectToType<SimpleRecipeForReportList>().FirstOrDefaultAsync(r => r.Id == report!.ItemId))!;
                    recipe.TotalPendingReports = await db.ViolationReports.Where(report => report.Status == (int)ReportStatusEnum.Pending && report.ItemId == recipe.Id).CountAsync();
                    isReportedRecipes!.Add(recipe);
                }
            try
            {
                //search
                if (s != null)
                {
                    isReportedRecipes = SearchRecipes(s, isReportedRecipes);
                }
                //filter
                if (filter != null && filter == RecipeStatusEnum.Active.ToString())
                {
                    isReportedRecipes = FilterByStatusActiveList(isReportedRecipes);
                }
                else if (filter != null && filter == RecipeStatusEnum.Inactive.ToString())
                {
                    isReportedRecipes = FilterByStatusInactive(isReportedRecipes);
                }
                //orderby
                if (sort != null && sort == RecipeOrderByEnum.DescName.ToString())
                {
                    isReportedRecipes = OrderByDescName(isReportedRecipes);
                }
                else if (sort != null && sort == RecipeOrderByEnum.AscName.ToString())
                {
                    isReportedRecipes = OrderByAscName(isReportedRecipes);
                }
                return isReportedRecipes!.Skip((page - 1) * size)
                                .Take(size).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return isReportedRecipes!;
        }

        public async Task<int?> CountTotalReportedRecipes(string? s, string? filter)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<ViolationReport> reportsRecipeType = await db.ViolationReports.Where(report => report.ItemType == (int)ReportTypeEnum.Recipe).GroupBy(x => x.ItemId).Select(d => d.First()).ToListAsync();
            List<SimpleRecipeForReportList>? isReportedRecipes = new List<SimpleRecipeForReportList>();
            if (reportsRecipeType.Count() > 0)
                foreach (var report in reportsRecipeType)
                {
                    isReportedRecipes!.Add((await db.Recipes.ProjectToType<SimpleRecipeForReportList>().FirstOrDefaultAsync(r => r.Id == report!.ItemId))!);
                }
            try
            {
                //search
                if (s != null)
                {
                    isReportedRecipes = SearchRecipes(s, isReportedRecipes);
                }
                //filter
                if (filter != null && filter == RecipeStatusEnum.Active.ToString())
                {
                    isReportedRecipes = FilterByStatusActiveList(isReportedRecipes);
                }
                else if (filter != null && filter == RecipeStatusEnum.Inactive.ToString())
                {
                    isReportedRecipes = FilterByStatusInactive(isReportedRecipes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return isReportedRecipes!.Count();
        }
    }
}
