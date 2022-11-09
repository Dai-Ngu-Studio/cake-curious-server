using BusinessObject;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Roles;
using Repository.Constants.Users;
using Repository.Interfaces;
using Repository.Models.Recipes;
using Repository.Models.Roles;
using Repository.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IUserDeviceRepository userDeviceRepository;
        private readonly IUserFollowRepository userFollowRepository;
        private readonly IRecipeRepository recipeRepository;

        public UsersController(IUserRepository _userRepository, IUserDeviceRepository _userDeviceRepository, IUserFollowRepository _userFollowRepository, IRecipeRepository _recipeRepository)
        {
            userRepository = _userRepository;
            userDeviceRepository = _userDeviceRepository;
            userFollowRepository = _userFollowRepository;
            recipeRepository = _recipeRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<AdminDashboardUserPage>> GetUsers(string? search, string? sort, string? filter, [Range(1, int.MaxValue)] int page = 1, [Range(1, int.MaxValue)] int size = 10)
        {
            var result = new AdminDashboardUserPage();
            result.Users = await userRepository.GetList(search, sort, filter, page, size);
            result.TotalPage = (int)Math.Ceiling((decimal)userRepository.CountDashboardUser(search, sort, filter) / size);
            return Ok(result);
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDetailForWeb?>> GetUser(string? id)
        {
            if (id == null)
            {
                return BadRequest("You need to input id");
            }
            return Ok(await userRepository.GetUserDetailForWeb(id!));
        }
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateUser(User user, string id)
        {
            try
            {
                if (id != user.Id) return BadRequest("Input id must match with id of input user obj");
                User? beforeUpdateObj = await userRepository.Get(user.Id);
                if (beforeUpdateObj == null) throw new Exception("user that need to update does not exist");
                User updateObj = new User()
                {
                    Address = user.Address == null ? beforeUpdateObj.Address : user.Address,
                    CitizenshipDate = user.CitizenshipDate == null ? beforeUpdateObj.CitizenshipDate : user.CitizenshipDate,
                    DateOfBirth = user.DateOfBirth == null ? beforeUpdateObj.DateOfBirth : user.DateOfBirth,
                    Email = user.Email == null ? beforeUpdateObj.Email : user.Email,
                    DisplayName = user.DisplayName == null ? beforeUpdateObj.DisplayName : user.DisplayName,
                    Gender = user.Gender == null ? beforeUpdateObj.Gender : user.Gender,
                    FullName = user.FullName == null ? beforeUpdateObj.FullName : user.FullName,
                    PhotoUrl = user.PhotoUrl == null ? beforeUpdateObj.PhotoUrl : user.PhotoUrl,
                    Status = user.Status == null ? beforeUpdateObj.Status : user.Status,
                    Id = user.Id == null ? beforeUpdateObj.Id : user.Id,
                    CitizenshipNumber = user.CitizenshipNumber == null ? beforeUpdateObj.CitizenshipNumber : user.CitizenshipNumber,
                };
                await userRepository.Update(updateObj);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (userRepository.Get(id) == null)
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<User?>> DeleteUser(string? id)
        {
            User? user = await userRepository.DeleteUser(id);
            return Ok("Delete User " + user!.DisplayName + " success");
        }
        [HttpGet("{id}/following")]
        [Authorize]
        public async Task<ActionResult<FollowUserPage>> GetFollowingOfUser(string id,
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var followUserPage = new FollowUserPage();
                followUserPage.TotalPages = (int)Math.Ceiling((decimal)await userRepository.CountFollowingOfUser(id) / take);
                followUserPage.Followings = await userRepository.GetFollowingOfUser(id, uid, (page - 1) * take, take);
                return Ok(followUserPage);
            }
            return Unauthorized();
        }

        [HttpGet("{id}/followers")]
        [Authorize]
        public async Task<ActionResult<FollowUserPage>> GetFollowersOfUser(string id,
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var followUserPage = new FollowUserPage();
                followUserPage.TotalPages = (int)Math.Ceiling((decimal)await userRepository.CountFollowersOfUser(id) / take);
                followUserPage.Followers = await userRepository.GetFollowersOfUser(id, uid, (page - 1) * take, take);
                return Ok(followUserPage);
            }
            return Unauthorized();
        }

        [HttpPost("{id}/follow")]
        [Authorize]
        public async Task<ActionResult> FollowUser(string id)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                if (await userFollowRepository.IsUserFollowedByFollower(id, uid))
                {
                    // Remove follow
                    try
                    {
                        await userFollowRepository.Remove(id, uid);
                        return Ok();
                    }
                    catch (Exception)
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    // Add follow
                    try
                    {
                        await userFollowRepository.Add(id, uid);
                        return Ok();
                    }
                    catch (Exception)
                    {
                        return BadRequest();
                    }
                }
            }
            return Unauthorized();
        }

        /// <summary>
        /// Add user role to current user
        /// </summary>
        /// <returns></returns>
        [HttpPost("current/to-user")]
        [Authorize]
        public async Task<ActionResult<DetachedUser>> AdduserRoleToCurrentUser()
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                // Check if user existed in database
                User? user = await userRepository.Get(uid);
                if (user != null)
                {
                    // Check if user already has user role
                    var roleExisted = user.HasRoles!.Any(x => x.RoleId == (int)RoleEnum.StoreOwner);
                    if (!roleExisted)
                    {
                        try
                        {
                            // Add role to user
                            user.HasRoles!.Add(new UserHasRole
                            {
                                UserId = uid,
                                RoleId = (int)RoleEnum.StoreOwner,
                            });
                            await userRepository.Update(user);
                            return Ok();
                        }
                        catch (Exception)
                        {
                            return BadRequest();
                        }
                    }
                    return BadRequest();
                }
            }
            return Unauthorized();
        }

        /// <summary>
        /// For development purpose only.
        /// </summary>
        /// <param name="roleRequest"></param>
        /// <returns></returns>
        [HttpPost("current/role")]
        [Authorize]
        public async Task<ActionResult<DetachedUser>> AddRoleToCurrentUser(AddRoleRequest roleRequest)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                // Check if user existed in database
                User? user = await userRepository.Get(uid);
                if (user != null)
                {
                    // Check if user already has requested role
                    var roleExisted = user.HasRoles!.Any(x => x.RoleId == roleRequest.RoleId);
                    if (!roleExisted)
                    {
                        try
                        {
                            // Add role to user
                            user.HasRoles!.Add(new UserHasRole
                            {
                                UserId = uid,
                                RoleId = roleRequest.RoleId,
                            });
                            await userRepository.Update(user);
                            return Ok();
                        }
                        catch (Exception)
                        {
                            return BadRequest();
                        }
                    }
                    return BadRequest();
                }
            }
            return Unauthorized();
        }

        [HttpPost("login")]
        [Authorize]
        public async Task<ActionResult<DetachedUser>> Login([FromHeader] string? FcmToken = "")
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                // Check if user existed in database
                DetachedUser? user = await userRepository.GetDetached(uid);
                if (user != null)
                {
                    // User already exists in database, check if device needed to be added
                    await CheckAndAddDevice(FcmToken, uid);
                    // Return user with no collection attached (except roles)
                    return Ok(user);
                }
                else
                {
                    // User does not exist in database, creating user in database
                    try
                    {
                        // Get user information from Firebase
                        UserRecord? userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
                        bool isUserRecordExisted = userRecord != null;
                        // Default role to be Baker
                        var hasRoles = new HashSet<UserHasRole>();
                        hasRoles.Add(new UserHasRole
                        {
                            UserId = uid,
                            RoleId = (int)RoleEnum.Baker,
                        });
                        User newUser = new User
                        {
                            Id = uid,
                            DisplayName = isUserRecordExisted
                            ? userRecord!.DisplayName != null
                            ? userRecord!.DisplayName
                            : "Anonymous User"
                            : "Anonymous User",
                            Username = uid,
                            Email = isUserRecordExisted ? userRecord!.Email : "Anonymous",
                            PhotoUrl = isUserRecordExisted ? userRecord!.PhotoUrl : "",
                            HasRoles = hasRoles,
                            CreatedDate = DateTime.Now,
                            Status = (int)UserStatusEnum.Active,
                        };
                        await userRepository.Add(newUser);
                        // Check if device needed to be added
                        await CheckAndAddDevice(FcmToken, uid);
                        // Return user with no collection attached (except roles)
                        user = await userRepository.GetDetached(uid);
                        return Ok(user);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return BadRequest(e.Message);
                    }
                }
            }
            return Unauthorized();
        }

        [HttpGet("current/bookmarks")]
        [Authorize]
        public async Task<ActionResult<HomeRecipePage>> GetBookmarksOfCurrentUser(
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var recipePage = new HomeRecipePage();
                recipePage.TotalPages = (int)Math.Ceiling((decimal)await recipeRepository.CountBookmarksOfUser(uid) / take);
                recipePage.Recipes = recipeRepository.GetBookmarksOfUser(uid, (page - 1) * take, take);
                return Ok(recipePage);
            }
            return Unauthorized();
        }

        [HttpGet("{id:length(1,128)}/likes")]
        [Authorize]
        public async Task<ActionResult<HomeRecipePage>> GetLikesOfUser(
            string id,
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    if (id == "current")
                    {
                        id = uid;
                    }
                    var recipePage = new HomeRecipePage();
                    recipePage.TotalPages = (int)Math.Ceiling((decimal)await recipeRepository.CountLikedOfUser(id) / take);
                    recipePage.Recipes = recipeRepository.GetLikedOfUser(id, (page - 1) * take, take);
                    return Ok(recipePage);
                }
                return BadRequest();
            }
            return Unauthorized();
        }

        [HttpGet("{id:length(1,128)}/profile")]
        [Authorize]
        public async Task<ActionResult<ProfileUser>> GetProfileUser(string id)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    if (id == "current")
                    {
                        id = uid;
                    }
                    return Ok(await userRepository.GetProfileUser(id, uid));
                }
                return BadRequest();
            }
            return Unauthorized();
        }

        [HttpGet("{id:length(1,128)}/recipes")]
        [Authorize]
        public async Task<ActionResult<HomeRecipePage>> GetRecipesOfUser(
            string id,
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    if (id == "current")
                    {
                        id = uid;
                    }
                    var recipePage = new HomeRecipePage();
                    recipePage.TotalPages = (int)Math.Ceiling((decimal)await recipeRepository.CountRecipesOfUser(id) / take);
                    recipePage.Recipes = recipeRepository.GetRecipesOfUser(id, (page - 1) * take, take);
                    return Ok(recipePage);
                }
                return BadRequest();
            }
            return Unauthorized();
        }

        private async Task CheckAndAddDevice(string? FcmToken, string uid)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(FcmToken))
                {
                    // Check if device existed in database
                    var userDevice = await userDeviceRepository.Get(FcmToken);
                    if (userDevice == null)
                    {
                        // Device does not exist, adding to database
                        var newUserDevice = new UserDevice
                        {
                            Token = FcmToken,
                            UserId = uid,
                        };
                        await userDeviceRepository.Add(newUserDevice);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
