using BusinessObject;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.Comments;
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

        public RecipesController(IRecipeRepository _recipeRepository, ICommentRepository _commentRepository)
        {
            recipeRepository = _recipeRepository;
            commentRepository = _commentRepository;
        }

        [HttpGet("following")]
        [Authorize]
        public ActionResult<HomeRecipePage> GetRecipesFromFollowing(
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var recipePage = new HomeRecipePage();
                recipePage.TotalPages = (int)Math.Ceiling((decimal)recipeRepository.CountLatestRecipesForFollower(uid) / take);
                recipePage.Recipes = recipeRepository.GetLatestRecipesForFollower(uid, (page - 1) * take, take);
                return Ok(recipePage);
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

                var recipe = createRecipe.Adapt<Recipe>();

                recipe.Status = 0;
                recipe.PublishedDate = DateTime.Now;
                recipe.UserId = uid;

                await recipeRepository.AddRecipe(recipe, materials);
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

        [HttpGet("{id:guid}/{step:int}")]
        [Authorize]
        public async Task<ActionResult<DetailRecipeStep>> GetRecipeStepDetails(Guid id, [Range(1, int.MaxValue)] int step)
        {
            try
            {
                var recipe = await recipeRepository.GetRecipeStepDetails(id, step);
                if (recipe != null)
                {
                    return Ok(recipe);
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
