using BusinessObject;
using CakeCurious_API.Utilities;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Constants.Comments;
using Repository.Constants.Reports;
using Repository.Constants.Roles;
using Repository.Interfaces;
using Repository.Models.Comments;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentRepository commentRepository;
        private readonly IUserRepository userRepository;
        private readonly IViolationReportRepository reportRepository;
        private readonly IUserDeviceRepository userDeviceRepository;
        private readonly IRecipeRepository recipeRepository;
        private readonly INotificationRepository notificationRepository;

        public CommentsController(ICommentRepository _commentRepository, IUserRepository _userRepository,
            IUserDeviceRepository _userDeviceRepository, INotificationRepository _notificationRepository,
            IRecipeRepository _recipeRepository, IViolationReportRepository _reportRepository)
        {
            commentRepository = _commentRepository;
            userRepository = _userRepository;
            userDeviceRepository = _userDeviceRepository;
            notificationRepository = _notificationRepository;
            recipeRepository = _recipeRepository;
            reportRepository = _reportRepository;
        }

        [HttpGet("Is-Reported")]
        [Authorize]
        public async Task<ActionResult<ReportedCommentsPage>> GetReportedComment(string? filter, string? sort, [Range(1, int.MaxValue)] int page = 1, [Range(1, int.MaxValue)] int size = 10)
        {
            ReportedCommentsPage reportedCommentsPage = new ReportedCommentsPage();
            try
            {
                reportedCommentsPage.Comments = await commentRepository.GetReportedCommments(filter, sort, page, size);
                Console.WriteLine(reportedCommentsPage.Comments.Count());
                reportedCommentsPage.TotalPage = (int)Math.Ceiling((decimal)await commentRepository.CountReportedCommmentsTotalPage(filter) / size);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while get reported comments: " + ex.Message);
                return BadRequest("Error while get reported comments list.");
            }
            return Ok(reportedCommentsPage);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<SimpleComment>> GetCommentById(Guid id)
        {
            SimpleComment comment = await commentRepository.GetCommentById(id);
            if (comment == null) return BadRequest("Not found comment.");
            return Ok(comment);
        }

        [HttpGet("{id:guid}/replies")]
        [Authorize]
        public async Task<ActionResult<CommentPage>> GetRepliesOfComment(Guid id,
        [Range(1, int.MaxValue)] int page = 1,
        [Range(1, int.MaxValue)] int take = 5)
        {
            var comment = await commentRepository.GetCommentReadonly(id);
            if (comment?.Status == (int)CommentStatusEnum.Active)
            {
                var commentPage = new CommentPage();
                commentPage.TotalPages = (int)Math.Ceiling((decimal)await commentRepository.CountRepliesForComment(id) / take);
                commentPage.Comments = commentRepository.GetRepliesForComment(id, (page - 1) * take, take);
                return Ok(commentPage);
            }
            return Conflict();
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<RecipeComment>> UpdateComment(Guid id, UpdateComment updateComment)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var comment = await commentRepository.GetCommentWithRootReadonly(id);
                if (comment != null)
                {
                    if (comment.Status == (int)CommentStatusEnum.Inactive
                        || (comment.Root != null && comment.Root.Status == (int)CommentStatusEnum.Inactive))
                    {
                        return Conflict();
                    }

                    if (comment.UserId == uid
                        || await UserRoleAuthorizer.AuthorizeUser(new RoleEnum[] { RoleEnum.Administrator, RoleEnum.Staff }, uid, userRepository))
                    {
                        updateComment.Content = updateComment.Content!.Trim();
                        var rows = await commentRepository.Update(id, updateComment);
                        return (rows > 0) ? Ok(await commentRepository.GetRecipeComment(id)) : BadRequest();
                    }
                }
                return NotFound();
            }
            return Forbid();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RecipeComment>> PostComment(CreateComment createComment)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                if (createComment.RootId != null)
                {
                    var rootComment = await commentRepository.GetCommentReadonly((Guid)createComment.RootId!);
                    if (rootComment?.Status == (int)CommentStatusEnum.Inactive)
                    {
                        return Conflict();
                    }
                }

                var comment = createComment.Adapt<Comment>();
                comment.UserId = uid;
                comment.SubmittedDate = DateTime.Now;
                comment.Status = (int)CommentStatusEnum.Active;
                comment.Content = comment.Content!.Trim();
                await commentRepository.Add(comment);
                return Ok(await commentRepository.GetRecipeComment((Guid)comment.Id!));
            }
            return Forbid();
        }

        [HttpDelete("take-down/{id:guid}")]
        [Authorize]
        public async Task<ActionResult> TakeDownAnComment(Guid? id)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id == null)
            {
                return BadRequest("Missing input id");
            }
            try
            {
                await commentRepository.Delete(id.Value);
            }
            catch (Exception)
            {

                return BadRequest("Error when delete an item");
            }
            try
            {
                if (uid != null)
                {
                    await reportRepository.UpdateAllReportStatusOfAnItem(id.Value, uid!);
                    _ = Task.Run(() => NotificationUtility
                        .NotifyReporters(userDeviceRepository, notificationRepository, reportRepository,
                            recipeRepository, commentRepository, id.Value, (int)ReportTypeEnum.Comment));
                }
            }
            catch (Exception)
            {
                return BadRequest("Error when change all reports status of an item to censored");
            }
            return Ok("Take down item success.");
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<ActionResult> DeleteComment(Guid id)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var comment = await commentRepository.GetCommentReadonly(id);
                if (comment != null)
                {
                    if (comment.Status == (int)CommentStatusEnum.Inactive)
                    {
                        return Conflict();
                    }

                    if (comment.UserId == uid
                        || await UserRoleAuthorizer.AuthorizeUser(new RoleEnum[] { RoleEnum.Administrator, RoleEnum.Staff }, uid, userRepository))
                    {
                        var rows = await commentRepository.Delete(id);
                        return (rows > 0) ? Ok() : BadRequest();
                    }
                }
                return NotFound();
            }
            return Forbid();
        }
    }
}
