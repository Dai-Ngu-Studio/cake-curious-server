using BusinessObject;
using CakeCurious_API.Utilities;
using Google.Apis.FirebaseDynamicLinks.v1;
using Google.Apis.FirebaseDynamicLinks.v1.Data;
using Google.Cloud.Translation.V2;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Repository.Constants.Recipes;
using Repository.Constants.Roles;
using Repository.Interfaces;
using Repository.Models.Bookmarks;
using Repository.Models.Comments;
using Repository.Models.Likes;
using Repository.Models.Recipes;
using Repository.Models.RecipeSteps;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeRepository recipeRepository;
        private readonly ICommentRepository commentRepository;
        private readonly ILikeRepository likeRepository;
        private readonly IBookmarkRepository bookmarkRepository;
        private readonly IUserRepository userRepository;
        private readonly IElasticClient elasticClient;
        private readonly FirebaseDynamicLinksService firebaseDynamicLinksService;
        private readonly IViolationReportRepository reportRepository;


        private readonly TranslationClient translationClient;

        public RecipesController(IRecipeRepository _recipeRepository, ICommentRepository _commentRepository,
            ILikeRepository _likeRepository, IBookmarkRepository _bookmarkRepository, IUserRepository _userRepository, IElasticClient _elasticClient, FirebaseDynamicLinksService _firebaseDynamicLinksService, TranslationClient _translationClient, IViolationReportRepository _reportRepository)
        {
            recipeRepository = _recipeRepository;
            commentRepository = _commentRepository;
            likeRepository = _likeRepository;
            bookmarkRepository = _bookmarkRepository;
            userRepository = _userRepository;
            elasticClient = _elasticClient;
            firebaseDynamicLinksService = _firebaseDynamicLinksService;
            reportRepository = _reportRepository;
            translationClient = _translationClient;
        }

        [HttpDelete("take-down/{guid}")]
        public async Task<ActionResult> TakeDownAnRecipe(Guid? guid)
        {
            if (guid == null)
            {
                return BadRequest("Missing input id");
            }
            try
            {
                await recipeRepository.Delete(guid.Value);
            }
            catch (Exception)
            {

                return BadRequest("Error when delete an item");
            }
            try
            {
                await reportRepository.UpdateAllReportStatusOfAnItem(guid.Value);

            }
            catch (Exception)
            {

                return BadRequest("Error when change all reports status of an item to censored");
            }
            return Ok("Take down item success.");
        }

        [HttpGet("Is-Reported")]
        [Authorize]
        public async Task<ActionResult<ReportedRecipesPage>> GetReportedRecipes(string? s, string? sort, string? filter, [Range(1, int.MaxValue)] int page = 1, [Range(1, int.MaxValue)] int size = 10)
        {
            ReportedRecipesPage reportedRecipesPage = new ReportedRecipesPage();
            reportedRecipesPage.Recipes = await recipeRepository.GetReportedRecipes(s, sort, filter, page, size);
            reportedRecipesPage.TotalPage = (int)Math.Ceiling((decimal)await recipeRepository.CountTotalReportedRecipes(s, sort, filter) / size);
            return Ok(reportedRecipesPage);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var recipe = await recipeRepository.GetRecipeReadonly(id);
                if (recipe != null)
                {
                    if (recipe.UserId == uid
                        || await UserRoleAuthorizer.AuthorizeUser(
                            new RoleEnum[] { RoleEnum.Administrator, RoleEnum.Staff }, uid, userRepository))
                    {
                        var rows = await recipeRepository.Delete(id);
                        if (rows > 0)
                        {
                            await elasticClient.DeleteAsync<ElasticsearchRecipe>(id);
                        }
                        return (rows > 0) ? Ok() : BadRequest();
                    }
                }
                return BadRequest();
            }
            return Unauthorized();
        }

        [HttpGet("explore")]
        [Authorize]
        public async Task<ExploreRecipes> Explore(int seed,
            [Range(1, int.MaxValue)] int take = 10,
            [Range(0, int.MaxValue)] int lastKey = 0)
        {
            var explore = new ExploreRecipes();
            explore.Recipes = await recipeRepository.Explore(seed, take, lastKey);
            return explore;
        }

        [HttpGet("following")]
        [Authorize]
        public async Task<ActionResult<HomeRecipePage>> GetRecipesFromFollowing(
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var recipePage = new HomeRecipePage();
                recipePage.TotalPages = (int)Math.Ceiling((decimal)await recipeRepository.CountLatestRecipesForFollower(uid) / take);
                recipePage.Recipes = recipeRepository.GetLatestRecipesForFollower(uid, (page - 1) * take, take);
                return Ok(recipePage);
            }
            return Unauthorized();
        }

        [HttpPost("{id:guid}/bookmark")]
        [Authorize]
        public async Task<ActionResult<DetachedBookmark>> Bookmark(Guid id)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                if (await bookmarkRepository.IsRecipeBookmarkedByUser(uid, id))
                {
                    // Remove bookmark
                    try
                    {
                        await bookmarkRepository.Remove(uid, id);
                        return Ok();
                    }
                    catch (Exception)
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    // Add bookmark
                    try
                    {
                        await bookmarkRepository.Add(uid, id);
                        return Ok();
                    }
                    catch (Exception)
                    {
                        // Recipe might not exist
                        return BadRequest();
                    }
                }
            }
            return Unauthorized();
        }

        [HttpPost("{id:guid}/like")]
        [Authorize]
        public async Task<ActionResult<DetachedLike>> Like(Guid id)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                if (await likeRepository.IsRecipeLikedByUser(uid, id))
                {
                    // Remove like
                    try
                    {
                        await likeRepository.Remove(uid, id);
                        return Ok();
                    }
                    catch (Exception)
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    // Add like
                    try
                    {
                        await likeRepository.Add(uid, id);
                        return Ok();
                    }
                    catch (Exception)
                    {
                        // Recipe might not exist
                        return BadRequest();
                    }
                }
            }
            return Unauthorized();
        }

        [HttpGet("{id:guid}/model")]
        [Authorize]
        public async Task<ActionResult<EditRecipe>> GetEditRecipe(Guid id)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var recipeReadonly = await recipeRepository.GetRecipeReadonly(id);
                if (recipeReadonly != null)
                {
                    if (recipeReadonly.UserId == uid
                        || await UserRoleAuthorizer.AuthorizeUser(
                            new RoleEnum[] { RoleEnum.Administrator, RoleEnum.Staff }, uid, userRepository))
                    {
                        return Ok(await recipeRepository.GetEditRecipe(id));
                    }
                }
            }
            return Unauthorized();
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<DetailRecipe>> UpdateRecipe(Guid id, UpdateRecipe updateRecipe)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var recipe = await recipeRepository.GetRecipe(id);
                if (recipe != null)
                {
                    if (recipe.UserId == uid
                        || await UserRoleAuthorizer.AuthorizeUser(
                            new RoleEnum[] { RoleEnum.Administrator, RoleEnum.Staff }, uid, userRepository))
                    {
                        var adaptedUpdateRecipe = updateRecipe.Adapt<Recipe>();
                        await recipeRepository.UpdateRecipe(recipe, adaptedUpdateRecipe);

                        var elastisearchMaterials = updateRecipe.Ingredients
                                .Select(x => x.MaterialName!).ToList();
                        var elastisearchCategories = updateRecipe.HasCategories!
                            .Where(x => x.RecipeCategoryId.HasValue)
                            .Select(x => x.RecipeCategoryId!.Value);

                        var esNames = new List<string>();
                        esNames.Add(updateRecipe.Name!);

                        // Translation
                        esNames = await TranslationHelper.TranslateSingle(translationClient, updateRecipe.Name!, esNames);
                        elastisearchMaterials = await TranslationHelper.TranslateList(translationClient, elastisearchMaterials, elastisearchMaterials);

                        var elastisearchRecipe = new ElasticsearchRecipe
                        {
                            Id = recipe.Id,
                            Name = esNames.ToArray(),
                            Materials = elastisearchMaterials.ToArray(),
                            Categories = elastisearchCategories.ToArray(),
                        };

                        // Does doc exist on Elasticsearch?
                        var existsResponse = await elasticClient.DocumentExistsAsync(new DocumentExistsRequest(index: "recipes", recipe.Id));
                        if (!existsResponse.Exists)
                        {
                            // Doc doesn't exist, create new
                            var asyncIndexResponse = await elasticClient.IndexDocumentAsync(elastisearchRecipe);
                        }
                        else
                        {
                            // Doc exists, update
                            var updateResponse = await elasticClient.UpdateAsync<ElasticsearchRecipe>(recipe.Id, x => x
                                    .Doc(elastisearchRecipe)
                                );
                        }

                        var dynamicLinkResponse = await CreateDynamicLink(recipe);
                        await recipeRepository.UpdateShareUrl((Guid)recipe.Id!, dynamicLinkResponse.ShortLink);

                        return Ok(await recipeRepository.GetRecipeDetails((Guid)recipe.Id!, uid));
                    }
                }
                return BadRequest();
            }
            return Unauthorized();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<DetailRecipe>> CreateRecipe(CreateRecipe createRecipe)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var recipe = createRecipe.Adapt<Recipe>();

                recipe.Status = (int)RecipeStatusEnum.Active;
                recipe.PublishedDate = DateTime.Now;
                recipe.UserId = uid;

                await recipeRepository.AddRecipe(recipe);

                var elastisearchMaterials = createRecipe.Ingredients
                    .Select(x => x.MaterialName!).ToList();
                var elastisearchCategories = createRecipe.HasCategories!
                    .Where(x => x.RecipeCategoryId.HasValue)
                    .Select(x => x.RecipeCategoryId!.Value);

                var esNames = new List<string>();
                esNames.Add(recipe.Name!);

                // Translate
                esNames = await TranslationHelper.TranslateSingle(translationClient, recipe.Name!, esNames);
                elastisearchMaterials = await TranslationHelper.TranslateList(translationClient, elastisearchMaterials, elastisearchMaterials);

                var elastisearchRecipe = new ElasticsearchRecipe
                {
                    Id = recipe.Id,
                    Name = esNames.ToArray(),
                    Materials = elastisearchMaterials.ToArray(),
                    Categories = elastisearchCategories.ToArray(),
                };

                var asyncIndexResponse = await elasticClient.IndexDocumentAsync(elastisearchRecipe);

                var dynamicLinkResponse = await CreateDynamicLink(recipe);
                await recipeRepository.UpdateShareUrl((Guid)recipe.Id!, dynamicLinkResponse.ShortLink);

                return Ok(await recipeRepository.GetRecipeDetails((Guid)recipe.Id!, uid));
            }
            return Unauthorized();
        }

        [HttpGet("trending")]
        [Authorize]
        public async Task<ActionResult<HomeRecipes>> GetTrendingRecipes(
            [Range(0, int.MaxValue)] int period = 0,
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            var recipePage = new HomeRecipePage();
            recipePage.TotalPages = (int)Math.Ceiling((decimal)await recipeRepository.CountTrendingRecipes(period) / take);
            recipePage.Recipes = recipeRepository.GetTrendingRecipes(period, (page - 1) * take, take);
            return Ok(recipePage);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<DetailRecipe>> GetRecipeDetails(Guid id)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var recipe = await recipeRepository.GetRecipeDetails(id, uid);
                if (recipe != null)
                {
                    return Ok(recipe);
                }
                return NotFound();
            }
            return Unauthorized();
        }

        [HttpGet("{id:guid}/steps")]
        [Authorize]
        public async Task<ActionResult<DetailRecipeSteps>> GetAllRecipeStepDetails(Guid id)
        {
            try
            {
                var recipe = await recipeRepository.GetRecipeWithStepsReadonly(id);
                if (recipe != null)
                {
                    var steps = new DetailRecipeSteps();
                    for (int i = 1; i <= recipe.RecipeSteps!.Count; i++)
                    {
                        var step = await recipeRepository.GetRecipeStepDetails(id, i);
                        if (step != null)
                        {
                            steps.RecipeSteps!.Enqueue(step);
                        }
                    }
                    return Ok(steps);
                }
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("{id:guid}/steps/{step:int}")]
        [Authorize]
        public async Task<ActionResult<DetailRecipeStep>> GetRecipeStepDetails(Guid id, [Range(1, int.MaxValue)] int step)
        {
            try
            {
                var recipeStep = await recipeRepository.GetRecipeStepDetails(id, step);
                if (recipeStep != null)
                {
                    return Ok(recipeStep);
                }
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("{id:guid}/comments")]
        [Authorize]
        public async Task<ActionResult<CommentPage>> GetCommentsOfRecipe(Guid id,
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            var commentPage = new CommentPage();
            commentPage.TotalPages = (int)Math.Ceiling((decimal)await commentRepository.CountCommentsForRecipe(id) / take);
            commentPage.Comments = commentRepository.GetCommentsForRecipe(id, (page - 1) * take, take);
            return Ok(commentPage);
        }

        private async Task<CreateShortDynamicLinkResponse> CreateDynamicLink(Recipe recipe)
        {
            return await DynamicLinkHelper.CreateDynamicLink(
                path: "recipe-details",
                linkService: firebaseDynamicLinksService,
                id: recipe.Id.ToString()!,
                name: recipe.Name!,
                description: recipe.Description ?? "",
                photoUrl: recipe.PhotoUrl ?? "",
                thumbnailUrl: recipe.ThumbnailUrl
            );
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<HomeRecipePage>> SearchRecipes(
            [FromQuery] string? query,
            [FromQuery] string[]? ingredients,
            [FromQuery] int[]? categories,
            [FromQuery] Guid? lastId,
            [FromQuery] double? lastScore,
            [Range(1, int.MaxValue)] int take = 5)
        {
            var searchDescriptor = new SearchDescriptor<ElasticsearchRecipe>();
            var descriptor = new QueryContainerDescriptor<ElasticsearchRecipe>();
            var shouldContainer = new List<QueryContainer>();
            var filterContainer = new List<QueryContainer>();

            if ((query == null || string.IsNullOrWhiteSpace(query))
                && (ingredients == null || ingredients.Length == 0)
                && (categories == null || categories.Length == 0))
            {
                var emptyPage = new HomeRecipePage();
                emptyPage.TotalPages = 0;
                emptyPage.Recipes = new List<HomeRecipe>();

                return Ok(emptyPage);
            }

            if (ingredients != null)
            {
                foreach (var ingredient in ingredients)
                {
                    if (!string.IsNullOrWhiteSpace(ingredient))
                    {
                        shouldContainer.Add(descriptor
                            .Match(m => m
                                .Field(f => f.Materials)
                                .Query(ingredient)
                                .Fuzziness(Fuzziness.EditDistance(2))
                            )
                        );
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                shouldContainer.Add(descriptor
                    .Match(m => m
                        .Field(f => f.Name)
                        .Query(query)
                        .Fuzziness(Fuzziness.EditDistance(2))
                    )
                );
            }

            if (categories != null)
            {
                shouldContainer.Add(descriptor
                    .Terms(x => x
                        .Field(f => f.Categories)
                        .Terms(categories)
                    )
                );

                filterContainer.Add(descriptor
                    .Terms(x => x
                        .Field(f => f.Categories)
                        .Terms(categories)
                    )
                );
            }

            if (lastId != null && lastScore != null)
            {
                searchDescriptor = searchDescriptor.SearchAfter(lastScore, lastId);
            }

            searchDescriptor = searchDescriptor.Index("recipes")
                .Size(take)
                .MinScore(0.01D)
                .Sort(ss => ss
                    .Descending(SortSpecialField.Score)
                    .Descending(f => f.Id.Suffix("keyword"))
                )
                .Query(q => q
                    .Bool(b => b
                        .Should(shouldContainer.ToArray())
                        .Filter(filterContainer.ToArray())
                    )
                );

            var searchResponse = await elasticClient.SearchAsync<ElasticsearchRecipe>(searchDescriptor);

            var elasticsearchRecipes = new List<KeyValuePair<Guid, double>>();

            foreach (var hit in searchResponse.Hits)
            {
                elasticsearchRecipes.Add(
                    new KeyValuePair<Guid, double>((Guid)hit.Source.Id!, (double)hit.Score!)
                );
            }

            var recipeIds = elasticsearchRecipes.Select(x => x.Key).ToList();
            var suggestedRecipes = await recipeRepository.GetSuggestedRecipes(recipeIds);

            var recipes = elasticsearchRecipes
                .Join(suggestedRecipes, es => es.Key, rc => (Guid)rc.Id!,
                    (es, rc) => { rc.Score = es.Value; return rc; });

            var recipePage = new HomeRecipePage();
            recipePage.Recipes = recipes;

            return Ok(recipePage);
        }
    }
}
