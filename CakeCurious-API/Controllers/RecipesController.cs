using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.Comments;
using Repository.Models.Recipes;
using Repository.Models.RecipeSteps;
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
        public ActionResult<ICollection<HomeRecipe>> GetRecipesFromFollowing(int skip = 0, int take = 5)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                return Ok(recipeRepository.GetLatestRecipesForFollower(uid, skip, take));
            }
            return Unauthorized();
        }

        [HttpGet("home")]
        [Authorize]
        public ActionResult<HomeRecipes> GetHomeRecipes()
        {
            return Ok(recipeRepository.GetHomeRecipes());
        }

        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<DetailRecipe> GetRecipeDetails(string id)
        {
            try
            {
                var guid = Guid.Parse(id);
                var recipe = recipeRepository.GetRecipeDetails(guid);
                if (recipe != null)
                {
                    return recipe;
                }
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("{id}/{step}")]
        [Authorize]
        public ActionResult<DetailRecipeStep> GetRecipeStepDetails(string id, int step)
        {
            try
            {
                var guid = Guid.Parse(id);
                var recipe = recipeRepository.GetRecipeStepDetails(guid, step);
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

        [HttpGet("{id}/comments")]
        [Authorize]
        public ActionResult<ICollection<RecipeComment>> GetCommentsOfRecipe(string id)
        {
            try
            {
                var guid = Guid.Parse(id);
                var comments = commentRepository.GetCommentsForRecipe(guid);
                return Ok(comments);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
