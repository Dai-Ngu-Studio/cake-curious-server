using BusinessObject;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Constants.Comments;
using Repository.Interfaces;
using Repository.Models.Comments;
using System.Security.Claims;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentRepository commentRepository;

        public CommentsController(ICommentRepository _commentRepository)
        {
            commentRepository = _commentRepository;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RecipeComment>> PostComment(CreateComment createComment)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                try
                {
                    var comment = createComment.Adapt<Comment>();
                    comment.UserId = uid;
                    comment.SubmittedDate = DateTime.Now;
                    comment.Status = (int)CommentStatusEnum.Active;
                    await commentRepository.Add(comment);
                    return Ok(comment.Adapt<RecipeComment>());
                }
                catch (Exception)
                {
                    return StatusCode(500);
                }
            }
            return Unauthorized();
        }
    }
}
