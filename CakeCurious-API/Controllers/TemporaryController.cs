using BusinessObject;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemporaryController : ControllerBase
    {
        private readonly CakeCuriousDbContext context;

        public TemporaryController(CakeCuriousDbContext _context)
        {
            context = _context;
        }

        [HttpGet("bookmark")]
        public Bookmark? GetBookmark()
        {
            return context.Bookmarks.FirstOrDefault();
        }

        [HttpGet("color")]
        public Color? GetColor()
        {
            return context.Colors.FirstOrDefault();
        }

        [HttpGet("comment")]
        public Comment? GetComment()
        {
            return context.Comments.FirstOrDefault();
        }

        [HttpGet("comment-media")]
        public CommentMedia? GetCommentMedia()
        {
            return context.CommentMedia.FirstOrDefault();
        }

        [HttpGet("coupon")]
        public Coupon? GetCoupon()
        {
            return context.Coupons.FirstOrDefault();
        }

        [HttpGet("like")]
        public Like? GetLike()
        {
            return context.Likes.FirstOrDefault();
        }

        [HttpGet("measurement")]
        public Measurement? GetMeasurement()
        {
            return context.Measurements.FirstOrDefault();
        }

        [HttpGet("order")]
        public Order? GetOrder()
        {
            return context.Orders.FirstOrDefault();
        }

        [HttpGet("order-detail")]
        public OrderDetail? GetOrderDetail()
        {
            return context.OrderDetails.FirstOrDefault();
        }

        [HttpGet("product")]
        public Product? GetProduct()
        {
            return context.Products.FirstOrDefault();
        }

        [HttpGet("product-category")]
        public ProductCategory? GetProductCategory()
        {
            return context.ProductCategories.FirstOrDefault();
        }

        [HttpGet("recipe")]
        public Recipe? GetRecipe()
        {
            return context.Recipes.FirstOrDefault();
        }

        [HttpGet("recipe-category")]
        public RecipeCategory? GetRecipeCategory()
        {
            return context.RecipeCategories.FirstOrDefault();
        }

        [HttpGet("recipe-category-group")]
        public RecipeCategoryGroup? GetRecipeCategoryGroup()
        {
            return context.RecipeCategoryGroups.FirstOrDefault();
        }

        [HttpGet("recipe-has-category")]
        public RecipeHasCategory? GetRecipeHasCategory()
        {
            return context.RecipeHasCategories.FirstOrDefault();
        }

        [HttpGet("recipe-material")]
        public RecipeMaterial? GetRecipeMaterial()
        {
            return context.RecipeMaterials.FirstOrDefault();
        }

        [HttpGet("recipe-media")]
        public RecipeMedia? GetRecipeMedia()
        {
            return context.RecipeMedia.FirstOrDefault();
        }

        [HttpGet("recipe-step")]
        public RecipeStep? GetRecipeStep()
        {
            return context.RecipeSteps.FirstOrDefault();
        }

        [HttpGet("recipe-step-material")]
        public RecipeStepMaterial? GetRecipeStepMaterial()
        {
            return context.RecipeStepMaterials.FirstOrDefault();
        }

        [HttpGet("report-category")]
        public ReportCategory? GetReportCategory()
        {
            return context.ReportCategories.FirstOrDefault();
        }

        [HttpGet("role")]
        public Role? GetRole()
        {
            return context.Roles.FirstOrDefault();
        }

        [HttpGet("store")]
        public Store? GetStore()
        {
            return context.Stores.FirstOrDefault();
        }

        [HttpGet("user")]
        public User? GetUser()
        {
            return context.Users.FirstOrDefault();
        }

        [HttpGet("user-device")]
        public UserDevice? GetUserDevice()
        {
            return context.UserDevices.FirstOrDefault();
        }

        [HttpGet("user-follow")]
        public UserFollow? GetUserFollow()
        {
            return context.UserFollows.FirstOrDefault();
        }

        [HttpGet("user-has-role")]
        public UserHasRole? GetUserHasRole()
        {
            return context.UserHasRoles.FirstOrDefault();
        }

        [HttpGet("violation-report")]
        public ViolationReport? GetViolationReport()
        {
            return context.ViolationReports.FirstOrDefault();
        }
    }
}
