﻿using BusinessObject;
using CakeCurious_API.Utilities;
using Google.Apis.FirebaseDynamicLinks.v1;
using Google.Apis.FirebaseDynamicLinks.v1.Data;
using Google.Cloud.Translation.V2;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Repository.Constants.Categories;
using Repository.Constants.Recipes;
using Repository.Constants.Reports;
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
        private readonly IViolationReportRepository reportRepository;
        private readonly IRecipeCategoryRepository recipeCategoryRepository;
        private readonly IUserDeviceRepository userDeviceRepository;
        private readonly INotificationRepository notificationRepository;
        private readonly IDeactivateReasonRepository deactivateReasonRepository;
        private readonly IElasticClient elasticClient;
        private readonly FirebaseDynamicLinksService firebaseDynamicLinksService;
        private readonly TranslationClient translationClient;

        public RecipesController(IRecipeRepository _recipeRepository, ICommentRepository _commentRepository,
            ILikeRepository _likeRepository, IBookmarkRepository _bookmarkRepository, IUserRepository _userRepository,
            IViolationReportRepository _reportRepository, IRecipeCategoryRepository _recipeCategoryRepository,
            IUserDeviceRepository _userDeviceRepository, INotificationRepository _notificationRepository,
            IDeactivateReasonRepository _deactivateReasonRepository,
            IElasticClient _elasticClient, FirebaseDynamicLinksService _firebaseDynamicLinksService, TranslationClient _translationClient)
        {
            recipeRepository = _recipeRepository;
            commentRepository = _commentRepository;
            likeRepository = _likeRepository;
            bookmarkRepository = _bookmarkRepository;
            userRepository = _userRepository;
            recipeCategoryRepository = _recipeCategoryRepository;
            reportRepository = _reportRepository;
            userDeviceRepository = _userDeviceRepository;
            notificationRepository = _notificationRepository;
            deactivateReasonRepository = _deactivateReasonRepository;
            elasticClient = _elasticClient;
            firebaseDynamicLinksService = _firebaseDynamicLinksService;
            translationClient = _translationClient;
        }

        [HttpDelete("take-down/{id:guid}")]
        [Authorize]
        public async Task<ActionResult> TakeDownAnRecipe(Guid? id)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id == null)
            {
                return BadRequest("Missing input id");
            }
            try
            {
                await recipeRepository.Delete(id.Value);
            }
            catch (Exception)
            {

                return BadRequest("Error when delete an item");
            }
            try
            {
                if (uid != null)
                {
                    await reportRepository.UpdateAllReportStatusOfAnItem(id.Value, uid!);
                    _ = Task.Run(() => NotificationUtility
                        .NotifyReporters(userDeviceRepository, notificationRepository, reportRepository,
                            recipeRepository, commentRepository, id.Value, (int)ReportTypeEnum.Recipe));
                    _ = Task.Run(() => NotificationUtility
                        .NotifyReported(userDeviceRepository, deactivateReasonRepository, notificationRepository, 
                            recipeRepository, commentRepository, id.Value, (int)ReportTypeEnum.Recipe));
                }
            }
            catch (Exception)
            {
                return BadRequest("Error when change all reports status of an item to censored");
            }
            return Ok("Take down item success.");
        }

        [HttpGet("Is-Reported")]
        [Authorize]
        public async Task<ActionResult<ReportedRecipesPage>> GetReportedRecipes(string? search, string? sort, string? filter, [Range(1, int.MaxValue)] int page = 1, [Range(1, int.MaxValue)] int size = 10)
        {
            ReportedRecipesPage reportedRecipesPage = new ReportedRecipesPage();
            try
            {
                reportedRecipesPage.Recipes = await recipeRepository.GetReportedRecipes(search, sort, filter, page, size);
                reportedRecipesPage.TotalPage = (int)Math.Ceiling((decimal)await recipeRepository.CountTotalReportedRecipes(search, filter) / size);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while get reported recipes: " + ex.Message);
                return BadRequest("Error while get reported recipes list.");
            }
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
                return NotFound();
            }
            return Forbid();
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
            return Forbid();
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
                    await bookmarkRepository.Remove(uid, id);
                    return Ok();
                }
                else
                {
                    try
                    {
                        await bookmarkRepository.Add(uid, id);
                        return Ok();
                    }
                    catch (Exception)
                    {
                        return NotFound();
                    }
                }
            }
            return Forbid();
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
                    await likeRepository.Remove(uid, id);
                    return Ok();
                }
                else
                {
                    try
                    {
                        await likeRepository.Add(uid, id);
                        return Ok();
                    }
                    catch (Exception)
                    {
                        return NotFound();
                    }
                }
            }
            return Unauthorized();
        }

        [HttpGet("{id:guid}/model")]
        [Authorize]
        public async Task<ActionResult<EditRecipe>> GetEditRecipe(Guid id, [FromQuery] int? la)
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
                        if (la == (int)CategoryLanguageEnum.English)
                        {
                            var recipe = await recipeRepository.GetEngEditRecipe(id);
                            var recipeCategoryGroups = recipeCategoryRepository.GetEnglishRecipeCategoriesGrouped().ToList();
                            var selectedCategories = recipe!.HasCategories;
                            foreach (var categoryGroup in recipeCategoryGroups)
                            {
                                var selectedRecipeCategoryIds = selectedCategories!
                                    .Join(categoryGroup.RecipeCategories!, sc => sc.RecipeCategoryId, rc => rc.Id, (sc, rc) => (int)rc.Id!);
                                categoryGroup.SelectedRecipeCategoryIds = selectedRecipeCategoryIds.ToArray();
                            }
                            recipe.RecipeCategoryGroups = recipeCategoryGroups;
                            return Ok(recipe);
                        }
                        else
                        {
                            var recipe = await recipeRepository.GetEditRecipe(id);
                            var recipeCategoryGroups = recipeCategoryRepository.GetRecipeCategoriesGrouped().ToList();
                            var selectedCategories = recipe!.HasCategories;
                            foreach (var categoryGroup in recipeCategoryGroups)
                            {
                                var selectedRecipeCategoryIds = selectedCategories!
                                    .Join(categoryGroup.RecipeCategories!, sc => sc.RecipeCategoryId, rc => rc.Id, (sc, rc) => (int)rc.Id!);
                                categoryGroup.SelectedRecipeCategoryIds = selectedRecipeCategoryIds.ToArray();
                            }
                            recipe.RecipeCategoryGroups = recipeCategoryGroups;
                            return Ok(recipe);
                        }
                    }
                }
                return NotFound();
            }
            return Forbid();
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

                        try
                        {
                            var elastisearchMaterials = updateRecipe.Ingredients
                                    .Select(x => x.MaterialName!.Trim()).ToList();
                            var elasticsearchEquipment = updateRecipe.Equipment
                                    .Select(x => x.MaterialName!.Trim()).ToList();
                            var elastisearchCategories = updateRecipe.HasCategories!
                                .Where(x => x.RecipeCategoryId.HasValue)
                                .Select(x => x.RecipeCategoryId!.Value);

                            var esNames = new List<string>();
                            esNames.Add(updateRecipe.Name!.Trim());

                            // Translation
                            esNames = await TranslationHelper.TranslateSingle(translationClient, updateRecipe.Name!, esNames);
                            elastisearchMaterials = await TranslationHelper.TranslateList(translationClient, elastisearchMaterials, elastisearchMaterials);
                            if (elasticsearchEquipment.Count > 0)
                            {
                                elasticsearchEquipment = await TranslationHelper.TranslateList(translationClient, elasticsearchEquipment, elasticsearchEquipment);
                            }

                            var elastisearchRecipe = new ElasticsearchRecipe
                            {
                                Id = recipe.Id,
                                Name = esNames.ToArray(),
                                Materials = elastisearchMaterials.ToArray(),
                                Equipment = elasticsearchEquipment.ToArray(),
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
                        }
                        catch (Exception)
                        {
                            // just catching so request does not fail because of failed translation or elasticsearch indexing
                        }

                        return Ok(await recipeRepository.GetRecipeDetails((Guid)recipe.Id!, uid));
                    }
                }
                return NotFound();
            }
            return Forbid();
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
                recipe.Name = recipe.Name!.Trim();
                recipe.Description = recipe.Description!.Trim();

                await recipeRepository.AddRecipe(recipe);

                try
                {
                    var elastisearchMaterials = createRecipe.Ingredients
                        .Select(x => x.MaterialName!.Trim()).ToList();
                    var elasticsearchEquipment = createRecipe.Equipment
                        .Select(x => x.MaterialName!.Trim()).ToList();
                    var elastisearchCategories = createRecipe.HasCategories!
                        .Where(x => x.RecipeCategoryId.HasValue)
                        .Select(x => x.RecipeCategoryId!.Value);

                    var esNames = new List<string>();
                    esNames.Add(recipe.Name!.Trim());

                    // Translate
                    esNames = await TranslationHelper.TranslateSingle(translationClient, recipe.Name!, esNames);
                    elastisearchMaterials = await TranslationHelper.TranslateList(translationClient, elastisearchMaterials, elastisearchMaterials);
                    if (elasticsearchEquipment.Count > 0)
                    {
                        elasticsearchEquipment = await TranslationHelper.TranslateList(translationClient, elasticsearchEquipment, elasticsearchEquipment);
                    }

                    var elastisearchRecipe = new ElasticsearchRecipe
                    {
                        Id = recipe.Id,
                        Name = esNames.ToArray(),
                        Materials = elastisearchMaterials.ToArray(),
                        Equipment = elasticsearchEquipment.ToArray(),
                        Categories = elastisearchCategories.ToArray(),
                    };

                    var asyncIndexResponse = await elasticClient.IndexDocumentAsync(elastisearchRecipe);

                    var dynamicLinkResponse = await CreateDynamicLink(recipe);
                    await recipeRepository.UpdateShareUrl((Guid)recipe.Id!, dynamicLinkResponse.ShortLink);

                }
                catch (Exception)
                {
                    // just catching so request does not fail because of failed translation or elasticsearch indexing
                }
                return Ok(await recipeRepository.GetRecipeDetails((Guid)recipe.Id!, uid));
            }
            return Forbid();
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
            return Forbid();
        }

        [HttpGet("{id:guid}/steps")]
        [Authorize]
        public async Task<ActionResult<DetailRecipeSteps>> GetAllRecipeStepDetails(Guid id)
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

        [HttpGet("{id:guid}/steps/{step:int}")]
        [Authorize]
        public async Task<ActionResult<DetailRecipeStep>> GetRecipeStepDetails(Guid id, [Range(1, int.MaxValue)] int step)
        {
            var recipeStep = await recipeRepository.GetRecipeStepDetails(id, step);
            if (recipeStep != null)
            {
                return Ok(recipeStep);
            }
            return NotFound();
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

            var recipePage = new SuggestRecipePage();
            recipePage.Recipes = recipes;

            return Ok(recipePage);
        }
    }
}
