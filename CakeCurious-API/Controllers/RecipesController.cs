using BusinessObject;
using CakeCurious_API.Utilities;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Constants.Recipes;
using Repository.Constants.Roles;
using Repository.Interfaces;
using Repository.Models.Bookmarks;
using Repository.Models.Comments;
using Repository.Models.Likes;
using Repository.Models.RecipeMaterials;
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

        public RecipesController(IRecipeRepository _recipeRepository, ICommentRepository _commentRepository,
            ILikeRepository _likeRepository, IBookmarkRepository _bookmarkRepository, IUserRepository _userRepository)
        {
            recipeRepository = _recipeRepository;
            commentRepository = _commentRepository;
            likeRepository = _likeRepository;
            bookmarkRepository = _bookmarkRepository;
            userRepository = _userRepository;
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
                        // New materials
                        var materials = new List<CreateRecipeMaterial>();
                        materials.AddRange(updateRecipe.Ingredients);
                        materials.AddRange(updateRecipe.Equipment);
                        foreach (var material in materials)
                        {
                            material.Id = Guid.NewGuid();
                        }
                        try
                        {
                            var adaptedUpdateRecipe = updateRecipe.Adapt<Recipe>();
                            await recipeRepository.UpdateRecipe(recipe, adaptedUpdateRecipe, materials);
                            return Ok(await recipeRepository.GetRecipeDetails((Guid)recipe.Id!, uid));
                        }
                        catch (Exception)
                        {
                            return StatusCode(500);
                        }
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
                var materials = new List<CreateRecipeMaterial>();
                materials.AddRange(createRecipe.Ingredients);
                materials.AddRange(createRecipe.Equipment);
                foreach (var material in materials)
                {
                    material.Id = Guid.NewGuid();
                }
                try
                {
                    var recipe = createRecipe.Adapt<Recipe>();

                    recipe.Status = (int)RecipeStatusEnum.Active;
                    recipe.PublishedDate = DateTime.Now;
                    recipe.UserId = uid;

                    await recipeRepository.AddRecipe(recipe, materials);
                    return Ok(await recipeRepository.GetRecipeDetails((Guid)recipe.Id!, uid));
                }
                catch (Exception)
                {
                    return StatusCode(500);
                }
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
        public ActionResult<IEnumerable<RecipeComment>> GetCommentsOfRecipe(Guid id)
        {
            try
            {
                var comments = commentRepository.GetCommentsForRecipe(id);
                return Ok(comments);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
