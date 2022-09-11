using BusinessObject;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using System.Security.Claims;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        public UsersController(IUserRepository _userRepository)
        {
            userRepository = _userRepository;
        }

        [HttpPost("login")]
        [Authorize]
        public async Task<ActionResult> Login([FromHeader] string? FcmToken = "")
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                User? user = await userRepository.Get(uid);
                if (user != null)
                {
                    try
                    {
                        UserRecord? userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
                        if (!string.IsNullOrWhiteSpace(FcmToken))
                        {
                            // to do add user device
                        }
                        return Ok();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return BadRequest(e.Message);
                    }
                }
                else
                {
                    try
                    {
                        UserRecord? userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
                        bool isUserRecordExisted = userRecord != null;
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
                            RoleId = 4,
                            Status = 0,
                        };
                        await userRepository.Add(newUser);
                        if (!string.IsNullOrWhiteSpace(FcmToken))
                        {
                            // to do add user device
                        }
                        return Ok();
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
    }
}
