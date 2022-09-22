using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.Recipes;
using System.Security.Claims;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeRepository recipeRepository;

        public RecipesController(IRecipeRepository _recipeRepository)
        {
            recipeRepository = _recipeRepository;
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
        public ActionResult<ICollection<ICollection<HomeRecipe>>> GetHomeRecipes()
        {
            return Ok(recipeRepository.GetHomeRecipes());
        }
    }
}
