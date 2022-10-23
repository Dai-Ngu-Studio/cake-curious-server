using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.RecipeCategories;
using Repository.Models.RecipeCategoryGroups;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeCategoriesController : ControllerBase
    {
        private readonly IRecipeCategoryRepository recipeCategoryRepository;

        public RecipeCategoriesController(IRecipeCategoryRepository _recipeCategoryRepository)
        {
            recipeCategoryRepository = _recipeCategoryRepository;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<DetachedRecipeCategories> GetCategories()
        {
            var recipeCategories = new DetachedRecipeCategories();
            recipeCategories.RecipeCategories = recipeCategoryRepository.GetRecipeCategories();
            return Ok(recipeCategories);
        }

        [HttpGet("grouped")]
        [Authorize]
        public ActionResult<DetachedRecipeCategories> GetCategoriesGrouped()
        {
            var recipeCategoriesGrouped = new DetachedRecipeCategoryGroups();
            recipeCategoriesGrouped.RecipeCategoryGroups = recipeCategoryRepository.GetRecipeCategoriesGrouped();
            return Ok(recipeCategoriesGrouped);
        }
    }
}
