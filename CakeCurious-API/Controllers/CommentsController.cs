using BusinessObject;
using CakeCurious_API.Utilities;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Constants.Comments;
using Repository.Constants.Roles;
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
        private readonly IUserRepository userRepository;

        public CommentsController(ICommentRepository _commentRepository, IUserRepository _userRepository)
        {
            commentRepository = _commentRepository;
            userRepository = _userRepository;
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<RecipeComment>> UpdateComment(Guid id, UpdateComment updateComment)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var comment = await commentRepository.GetCommentReadonly(id);
                if (comment != null)
                {
                    if (comment.UserId == uid
                        || await UserRoleAuthorizer.AuthorizeUser(new RoleEnum[] { RoleEnum.Administrator, RoleEnum.Staff }, uid, userRepository))
                    {
                        var rows = await commentRepository.Update(id, updateComment);
                        return (rows > 0) ? Ok(await commentRepository.GetRecipeComment(id)) : BadRequest();
                    }
                }
                return BadRequest();
            }
            return Unauthorized();
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
                    return Ok(await commentRepository.GetRecipeComment((Guid)comment.Id!));
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
