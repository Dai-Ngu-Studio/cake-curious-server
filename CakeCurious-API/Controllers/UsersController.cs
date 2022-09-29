using BusinessObject;
using CakeCurious_API.Utilities;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Constants.Roles;
using Repository.Constants.Users;
using Repository.Interfaces;
using Repository.Models.Roles;
using Repository.Models.Users;
using System.Security.Claims;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IUserDeviceRepository userDeviceRepository;

        public UsersController(IUserRepository _userRepository, IUserDeviceRepository _userDeviceRepository)
        {
            userRepository = _userRepository;
            userDeviceRepository = _userDeviceRepository;
        }

        /// <summary>
        /// Add store role to current user
        /// </summary>
        /// <returns></returns>
        [HttpPost("current/to-store")]
        [Authorize]
        public async Task<ActionResult<DetachedUser>> AddStoreRoleToCurrentUser()
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                // Check if user existed in database
                User? user = await userRepository.Get(uid);
                if (user != null)
                {
                    // Check if user already has store role
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
                            : userRecord!.Email
                            : "Anonymous User",
                            Email = isUserRecordExisted ? userRecord!.Email : "Anonymous",
                            PhotoUrl = isUserRecordExisted ? userRecord!.PhotoUrl : "",
                            HasRoles = hasRoles,
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
