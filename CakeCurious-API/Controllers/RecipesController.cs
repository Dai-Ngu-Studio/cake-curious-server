using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.Comments;
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
        public ActionResult<ICollection<HomeRecipe>> GetRecipesFromFollowing(
            [Range(1, int.MaxValue)] int page = 1, 
            [Range(0, int.MaxValue)] int take = 5)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(uid))
            {
                return Ok(recipeRepository.GetLatestRecipesForFollower(uid, (page - 1) * take, take));
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
        public ActionResult<DetailRecipe> GetRecipeDetails(Guid id)
        {
            try
            {
                var recipe = recipeRepository.GetRecipeDetails(id);
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

        [HttpGet("{id:guid}/{step:int}")]
        [Authorize]
        public ActionResult<DetailRecipeStep> GetRecipeStepDetails(Guid id, [Range(1, int.MaxValue)] int step)
        {
            try
            {
                var recipe = recipeRepository.GetRecipeStepDetails(id, step);
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
        public ActionResult<ICollection<RecipeComment>> GetCommentsOfRecipe(Guid id)
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
