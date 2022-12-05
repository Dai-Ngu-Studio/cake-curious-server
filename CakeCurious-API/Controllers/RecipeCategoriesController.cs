using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Constants.Categories;
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
        public ActionResult<DetachedRecipeCategories<DetachedRecipeCategory>> GetCategories(int la)
        {
            var recipeCategories = new DetachedRecipeCategories<DetachedRecipeCategory>();
            recipeCategories.RecipeCategories = (la == (int)CategoryLanguageEnum.English)
                ? recipeCategoryRepository.GetEnglishRecipeCategories()
                : recipeCategoryRepository.GetRecipeCategories();
            return Ok(recipeCategories);
        }

        [HttpGet("grouped")]
        [Authorize]
        public ActionResult<DetachedRecipeCategoryGroups> GetCategoriesGrouped(int la)
        {
            if (la == (int)CategoryLanguageEnum.English)
            {
                var recipeCategoriesGrouped = new EngDetachedRecipeCategoryGroups();
                recipeCategoriesGrouped.RecipeCategoryGroups =
                    recipeCategoryRepository.GetEnglishRecipeCategoriesGrouped();
                return Ok(recipeCategoriesGrouped);
            }
            else
            {
                var recipeCategoriesGrouped = new DetachedRecipeCategoryGroups();
                recipeCategoriesGrouped.RecipeCategoryGroups =
                    recipeCategoryRepository.GetRecipeCategoriesGrouped();
                return Ok(recipeCategoriesGrouped);
            }
        }
    }
}
