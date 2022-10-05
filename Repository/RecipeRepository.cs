using BusinessObject;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Recipes;
using Repository.Interfaces;
using Repository.Models.RecipeMaterials;
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

        public async Task<Recipe?> GetRecipeReadonly(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Recipes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<int> Delete(Guid id)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                string query = $"update [Recipe] set [Recipe].status = {(int)RecipeStatusEnum.Inactive} where [Recipe].id = '{id}'";
                var rows = await db.Database.ExecuteSqlRawAsync(query);
                await transaction.CommitAsync();
                return rows;
            }
        }

        public async Task<ICollection<ExploreRecipe>> Explore(int randSeed, int take, int key)
        {
            var result = new List<ExploreRecipe>();
            var db = new CakeCuriousDbContext();
            string query = $"select top {take} [Recipe].id, [Recipe].name, [Recipe].photo_url, abs(checksum([Recipe].id, rand(@randSeed)*rand(@randSeed))) as [key] from [Recipe] where abs(checksum([Recipe].id, rand(@randSeed)*rand(@randSeed))) > @key and [Recipe].status = {(int)RecipeStatusEnum.Active} order by abs(checksum([Recipe].id, rand(@randSeed)*rand(@randSeed)))";
            var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.Add(new SqlParameter("@randSeed", randSeed));
            cmd.Parameters.Add(new SqlParameter("@key", key));
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

        public async Task UpdateRecipe(Recipe recipe, Recipe updateRecipe, IEnumerable<CreateRecipeMaterial> recipeMaterials)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                // Set new information
                recipe.Name = updateRecipe.Name;
                recipe.Description = updateRecipe.Description;
                recipe.ServingSize = updateRecipe.ServingSize;
                recipe.PhotoUrl = updateRecipe.PhotoUrl;
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

                // Add relationship between material and step
                var stepMaterials = new List<RecipeStepMaterial>();
                foreach (var material in recipeMaterials)
                {
                    if (material.UsedInSteps != null)
                    {
                        foreach (var step in material.UsedInSteps)
                        {
                            var recipeStep = recipe.RecipeSteps!.FirstOrDefault(x => x.StepNumber == step);
                            if (recipeStep != null)
                            {
                                stepMaterials.Add(new RecipeStepMaterial
                                {
                                    RecipeMaterialId = material.Id,
                                    RecipeStepId = recipeStep.Id,
                                });
                            }
                        }
                    }
                }
                await db.RecipeStepMaterials.AddRangeAsync(stepMaterials);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
        }

        public async Task AddRecipe(Recipe recipe, IEnumerable<CreateRecipeMaterial> recipeMaterials)
        {
            var db = new CakeCuriousDbContext();
            // Add recipe, materials and steps
            await db.Recipes.AddAsync(recipe);
            // Add relationship between material and step
            var stepMaterials = new List<RecipeStepMaterial>();
            foreach (var material in recipeMaterials)
            {
                if (material.UsedInSteps != null)
                {
                    foreach (var step in material.UsedInSteps)
                    {
                        var recipeStep = recipe.RecipeSteps!.FirstOrDefault(x => x.StepNumber == step);
                        if (recipeStep != null)
                        {
                            stepMaterials.Add(new RecipeStepMaterial
                            {
                                RecipeMaterialId = material.Id,
                                RecipeStepId = recipeStep.Id,
                            });
                        }
                    }
                }
            }
            await db.RecipeStepMaterials.AddRangeAsync(stepMaterials);
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
                .OrderBy(x => x.Id)
                .Where(x => x.PublishedDate!.Value <= DateTime.Now
                && x.PublishedDate!.Value >= DateTime.Now.AddDays(-2))
                .Where(x => x.User!.Followers!.Any(x => x.FollowerId == uid))
                .Where(x => x.Status == (int)RecipeStatusEnum.Active)
                .CountAsync();
        }

        // Recipe was published within 2 days
        // Recipe has an author which is followed by the follower with UID
        public IEnumerable<HomeRecipe> GetLatestRecipesForFollower(string uid, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Recipes
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Where(x => x.PublishedDate!.Value <= DateTime.Now
                && x.PublishedDate!.Value >= DateTime.Now.AddDays(-2))
                .Where(x => x.User!.Followers!.Any(x => x.FollowerId == uid))
                .Where(x => x.Status == (int)RecipeStatusEnum.Active)
                .Skip(skip)
                .Take(take)
                .ProjectToType<HomeRecipe>();
        }

        // 10 recipes for each collection
        // Collections:
        // Trending - Most liked within 1 day
        public HomeRecipes GetHomeRecipes()
        {
            var db = new CakeCuriousDbContext();
            var home = new HomeRecipes();
            var trending = db.Recipes
                .AsNoTracking()
                .OrderByDescending(x => x.Likes!.Count)
                .Where(x => x.PublishedDate!.Value <= DateTime.Now
                && x.PublishedDate!.Value >= DateTime.Now.AddDays(-1))
                .Where(x => x.Status == (int)RecipeStatusEnum.Active)
                .Take(10)
                .ProjectToType<HomeRecipe>();
            home.Trending = trending;
            return home;
        }
    }
}
