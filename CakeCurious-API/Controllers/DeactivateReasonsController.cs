using BusinessObject;
using CakeCurious_API.Utilities;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Constants.DeactivateReasons;
using Repository.Constants.Roles;
using Repository.Interfaces;
using Repository.Models.DeactivateReasons;
using Repository.Models.Users;
using System.Security.Claims;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeactivateReasonsController : ControllerBase
    {
        private readonly IDeactivateReasonRepository deactivateReasonRepository;
        private readonly IRecipeRepository recipeRepository;
        private readonly ICommentRepository commentRepository;
        private readonly IUserRepository userRepository;
        private readonly IStoreRepository storeRepository;

        public DeactivateReasonsController(IDeactivateReasonRepository _deactivateReasonRepository,
            IRecipeRepository _recipeRepository, ICommentRepository _commentRepository,
            IUserRepository _userRepository, IStoreRepository _storeRepository)
        {
            deactivateReasonRepository = _deactivateReasonRepository;
            recipeRepository = _recipeRepository;
            commentRepository = _commentRepository;
            userRepository = _userRepository;
            storeRepository = _storeRepository;
        }

        [HttpPost("by-email")]
        public async Task<ActionResult> GetDeactivateReasonForEmail(EmailUser emailUser)
        {
            var user = await userRepository.GetReadonlyUserByEmail(emailUser.Email!);
            if (user != null)
            {
                var guid = ConvertUtility.ToGuid(user.Id!);
                return Ok(await deactivateReasonRepository.GetReasonByItemIdReadonly(guid));
            }
            return NotFound();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateReason(CreateDeactivateReason reason)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                if (await UserRoleAuthorizer
                .AuthorizeUser(new RoleEnum[] { RoleEnum.Administrator, RoleEnum.Staff }, uid!, userRepository))
                {
                    var deactivateReason = reason.Adapt<DeactivateReason>();
                    if (deactivateReason.ItemType != (int)ReasonItemTypeEnum.User && deactivateReason.ItemId == null)
                    {
                        return BadRequest(new { Error = "The field \"ItemId\" is required for item type 0, 1 and 3." });
                    }
                    switch (deactivateReason.ItemType)
                    {
                        case (int)ReasonItemTypeEnum.User:
                            if (string.IsNullOrEmpty(reason.UserId))
                            {
                                return BadRequest(new { Error = "The field \"UserId\" is required for item type 2 (User)." });
                            }

                            if (await userRepository.IsUserExisted(reason.UserId))
                            {
                                deactivateReason.ItemId = ConvertUtility.ToGuid(reason.UserId);
                                break;
                            }
                            return BadRequest(new { Error = "User does not exist." });
                        case (int)ReasonItemTypeEnum.Recipe:
                            if (!await recipeRepository.IsRecipeExisted((Guid)deactivateReason.ItemId!))
                            {
                                return BadRequest(new { Error = "Recipe does not exist." });
                            }
                            break;
                        case (int)ReasonItemTypeEnum.Comment:
                            if (!await commentRepository.IsCommentExisted((Guid)deactivateReason.ItemId!))
                            {
                                return BadRequest(new { Error = "Comment does not exist." });
                            }
                            break;
                        case (int)ReasonItemTypeEnum.Store:
                            if (!await storeRepository.IsStoreExisted((Guid)deactivateReason.ItemId!))
                            {
                                return BadRequest(new { Error = "Store does not exist." });
                            }
                            break;
                        default:
                            return BadRequest(new { Error = "Invalid item type." });
                    }

                    if (deactivateReason.ItemId != null && !string.IsNullOrWhiteSpace(deactivateReason.Reason))
                    {
                        deactivateReason.StaffId = uid;
                        deactivateReason.DeactivateDate = DateTime.Now;
                        deactivateReason.Reason = deactivateReason.Reason!.Trim();
                        await deactivateReasonRepository.CreateReason(deactivateReason);
                        return Ok();
                    }
                }
            }
            return Forbid();
        }
    }
}
