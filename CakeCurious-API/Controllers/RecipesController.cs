using BusinessObject;
using CakeCurious_API.Utilities;
using Google.Apis.FirebaseDynamicLinks.v1;
using Google.Apis.FirebaseDynamicLinks.v1.Data;
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

        public RecipesController(IRecipeRepository _recipeRepository, ICommentRepository _commentRepository,
            ILikeRepository _likeRepository, IBookmarkRepository _bookmarkRepository, IUserRepository _userRepository, IElasticClient _elasticClient, FirebaseDynamicLinksService _firebaseDynamicLinksService)
        {
            recipeRepository = _recipeRepository;
            commentRepository = _commentRepository;
            likeRepository = _likeRepository;
            bookmarkRepository = _bookmarkRepository;
            userRepository = _userRepository;
            elasticClient = _elasticClient;
            firebaseDynamicLinksService = _firebaseDynamicLinksService;
        }

        private async Task<CreateShortDynamicLinkResponse> CreateDynamicLink(Recipe recipe)
        {
            var webAppUri = Environment.GetEnvironmentVariable(EnvironmentHelper.WebAppUri);
            var sharePrefixUri = Environment.GetEnvironmentVariable(EnvironmentHelper.ShareUriPrefix);
            var androidPackageName = Environment.GetEnvironmentVariable(EnvironmentHelper.AndroidPackageName);
            var androidMinPackageVersion = Environment.GetEnvironmentVariable(EnvironmentHelper.AndroidMinPackageVersionCode);
            var androidFallbackLink = Environment.GetEnvironmentVariable(EnvironmentHelper.AndroidFallbackLink);
            var suffixOption = Environment.GetEnvironmentVariable(EnvironmentHelper.SuffixOption);

            var linkRequest = firebaseDynamicLinksService.ShortLinks.Create(new CreateShortDynamicLinkRequest
            {
                DynamicLinkInfo = new DynamicLinkInfo
                {
                    Link = $"{webAppUri}/recipe-details/{recipe.Id}/?name={recipe.Name}&photoUrl={recipe.PhotoUrl}",
                    DomainUriPrefix = sharePrefixUri,
                    AndroidInfo = new AndroidInfo
                    {
                        AndroidPackageName = androidPackageName,
                        AndroidMinPackageVersionCode = androidMinPackageVersion,
                        AndroidFallbackLink = androidFallbackLink,
                    },
                    SocialMetaTagInfo = new SocialMetaTagInfo
                    {
                        SocialTitle = recipe.Name,
                        SocialImageLink = !string.IsNullOrWhiteSpace(recipe.ThumbnailUrl) ? recipe.ThumbnailUrl: recipe.PhotoUrl,
                        SocialDescription = recipe.Description,
                    }
                },
                Suffix = new Suffix
                {
                    Option = suffixOption
                }
            });
            return await linkRequest.ExecuteAsync();
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
                            await elasticClient.DeleteAsync<ElastisearchRecipe>(id);
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
            explore.Explore = await recipeRepository.Explore(seed, take, lastKey);
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
                                .Select(x => x.MaterialName);
                        var elastisearchCategories = updateRecipe.HasCategories!
                            .Where(x => x.RecipeCategoryId.HasValue)
                            .Select(x => x.RecipeCategoryId!.Value);

                        var elastisearchRecipe = new ElastisearchRecipe
                        {
                            Id = recipe.Id,
                            Name = new string[] { updateRecipe.Name! },
                            Materials = elastisearchMaterials.ToArray()!,
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
                            var updateResponse = await elasticClient.UpdateAsync<ElastisearchRecipe>(recipe.Id, x => x
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
                    .Select(x => x.MaterialName);
                var elastisearchCategories = createRecipe.HasCategories!
                    .Where(x => x.RecipeCategoryId.HasValue)
                    .Select(x => x.RecipeCategoryId!.Value);

                var elastisearchRecipe = new ElastisearchRecipe
                {
                    Id = recipe.Id,
                    Name = new string[] { createRecipe.Name! },
                    Materials = elastisearchMaterials.ToArray()!,
                    Categories = elastisearchCategories.ToArray(),
                };

                var asyncIndexResponse = await elasticClient.IndexDocumentAsync(elastisearchRecipe);

                var dynamicLinkResponse = await CreateDynamicLink(recipe);

                await recipeRepository.UpdateShareUrl((Guid)recipe.Id!, dynamicLinkResponse.ShortLink);

                return Ok(await recipeRepository.GetRecipeDetails((Guid)recipe.Id!, uid));
            }
            return Unauthorized();
        }

        [HttpGet("home")]
        [Authorize]
        public ActionResult<HomeRecipes> GetHomeRecipes()
        {
            return Ok(recipeRepository.GetHomeRecipes());
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<DetailRecipe>> GetRecipeDetails(Guid id)
        {
            try
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
            catch (Exception)
            {
                return BadRequest();
            }
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

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<HomeRecipePage>> SearchRecipes(
            [FromQuery] string? query,
            [FromQuery] string[]? ingredients,
            [FromQuery] int[]? categories,
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            var searchDescriptor = new SearchDescriptor<ElastisearchRecipe>();
            var descriptor = new QueryContainerDescriptor<ElastisearchRecipe>();
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

            var countResponse = await elasticClient.CountAsync<ElastisearchRecipe>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Should(shouldContainer.ToArray())
                        .Filter(filterContainer.ToArray())
                    )
                )
            );

            var searchResponse = await elasticClient.SearchAsync<ElastisearchRecipe>(s => s
                .From((page - 1) * take)
                .Size(take)
                .MinScore(0.01D)
                .Sort(ss => ss
                    .Descending(SortSpecialField.Score)
                )
                .Query(q => q
                    .Bool(b => b
                        .Should(shouldContainer.ToArray())
                        .Filter(filterContainer.ToArray())
                    )
                )
            );

            var recipeIds = new List<Guid>();
            var recipes = new List<HomeRecipe>();

            foreach (var doc in searchResponse.Documents)
            {
                recipeIds.Add((Guid)doc.Id!);
            }

            var suggestedRecipes = await recipeRepository.GetSuggestedRecipes(recipeIds);

            foreach (var recipeId in recipeIds)
            {
                recipes.Add(suggestedRecipes.FirstOrDefault(x => x.Id == recipeId)!);
            }

            var recipePage = new HomeRecipePage();
            recipePage.TotalPages = (int)Math.Ceiling((decimal)countResponse.Count / take);
            recipePage.Recipes = recipes;

            return Ok(recipePage);
        }
    }
}
