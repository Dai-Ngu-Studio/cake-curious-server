using BusinessObject;
using CakeCurious_API.Utilities;
using FirebaseAdmin.Auth;
using Google.Apis.FirebaseDynamicLinks.v1;
using Google.Apis.FirebaseDynamicLinks.v1.Data;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using Repository.Constants.Exceptions;
using Repository.Constants.Products;
using Repository.Constants.Roles;
using Repository.Constants.Users;
using Repository.Interfaces;
using Repository.Models.Notifications;
using Repository.Models.Orders;
using Repository.Models.Recipes;
using Repository.Models.Stores;
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
        private readonly IStoreRepository storeRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IDeactivateReasonRepository deactivateReasonRepository;
        private readonly INotificationRepository notificationRepository;
        private readonly IElasticClient elasticClient;
        private readonly FirebaseDynamicLinksService firebaseDynamicLinksService;

        public UsersController(IUserRepository _userRepository, IUserDeviceRepository _userDeviceRepository,
            IUserFollowRepository _userFollowRepository, IRecipeRepository _recipeRepository, IStoreRepository _storeRepository,
            IOrderRepository _orderRepository, IDeactivateReasonRepository _deactivateReasonRepository,
            INotificationRepository _notifcationRepository,
            IElasticClient _elasticClient, FirebaseDynamicLinksService _firebaseDynamicLinksService)
        {
            userRepository = _userRepository;
            userDeviceRepository = _userDeviceRepository;
            userFollowRepository = _userFollowRepository;
            recipeRepository = _recipeRepository;
            storeRepository = _storeRepository;
            orderRepository = _orderRepository;
            deactivateReasonRepository = _deactivateReasonRepository;
            notificationRepository = _notifcationRepository;
            elasticClient = _elasticClient;
            firebaseDynamicLinksService = _firebaseDynamicLinksService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<AdminDashboardUserPage> GetUsers(string? search, string? sort, string? filter, [Range(1, int.MaxValue)] int page = 1, [Range(1, int.MaxValue)] int size = 10)
        {
            var result = new AdminDashboardUserPage();
            result.Users = userRepository.GetList(search, sort, filter, page, size);
            result.TotalPage = (int)Math.Ceiling((decimal)userRepository.CountDashboardUser(search, filter) / size);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDetailForWeb?>> GetUser(string? id)
        {
            UserDetailForWeb? user = await userRepository.GetUserDetailForWeb(id!);
            if (user == null)
                return BadRequest("User not found.");
            return Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateUser(UpdateUser newUser, string id)
        {
            User updateUser;
            try
            {
                if (id != newUser.Id) return BadRequest("Input id must match with id of input user obj");
                User? user = await userRepository.Get(newUser.Id);
                if (user == null) return BadRequest("user that need to update does not exist");
                var initialStatus = user!.Status;
                updateUser = new User()
                {
                    Address = newUser.Address ?? user.Address,
                    CitizenshipDate = newUser.CitizenshipDate ?? user.CitizenshipDate,
                    DateOfBirth = newUser.DateOfBirth ?? user.DateOfBirth,
                    Email = newUser.Email ?? user.Email,
                    DisplayName = newUser.DisplayName ?? user.DisplayName,
                    Gender = newUser.Gender ?? user.Gender,
                    FullName = newUser.FullName ?? user.FullName,
                    PhotoUrl = newUser.PhotoUrl ?? user.PhotoUrl,
                    PhoneNumber = newUser.PhoneNumber ?? user.PhoneNumber,
                    Status = newUser.Status ?? user.Status,
                    Id = newUser.Id ?? user.Id,
                    CitizenshipNumber = newUser.CitizenshipNumber ?? user.CitizenshipNumber,
                    CreatedDate = user.CreatedDate,
                    StoreId = user.StoreId,
                    Username = user.Username,
                };
                if (newUser.Roles != null)
                    foreach (int role in newUser!.Roles!)
                    {
                        updateUser.HasRoles!.Add(new UserHasRole
                        {
                            UserId = newUser.Id,
                            RoleId = role,
                        });
                    }
                var dynamicLinkResponse = await CreateUserDynamicLink(updateUser);
                updateUser.ShareUrl = dynamicLinkResponse.ShortLink;

                await userRepository.Update(updateUser);

                try
                {
                    if (updateUser.Status != initialStatus)
                    {
                        var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(updateUser.Id);
                        if (userRecord != null)
                        {
                            var args = new UserRecordArgs
                            {
                                Uid = userRecord.Uid,
                            };
                            switch (updateUser.Status)
                            {
                                case (int)UserStatusEnum.Inactive:
                                    args.Disabled = true;
                                    break;
                                case (int)UserStatusEnum.Active:
                                    args.Disabled = false;
                                    break;
                            }
                            await FirebaseAuth.DefaultInstance.UpdateUserAsync(args);
                        }
                    }
                }
                catch (Exception)
                {
                    // do nothing
                }

                var elasticsearchUser = new ElasticsearchUser
                {
                    Id = user.Id,
                    Username = updateUser.Username,
                    DisplayName = updateUser.DisplayName,
                    Roles = user.HasRoles!.Select(x => (int)x.RoleId!).ToArray(),
                };

                // Does doc exist on Elasticsearch?
                var existsResponse = await elasticClient.DocumentExistsAsync(new DocumentExistsRequest(index: "users", updateUser.Id));
                if (!existsResponse.Exists)
                {
                    // Doc doesn't exist, create new
                    var createResponse = await elasticClient.CreateAsync<ElasticsearchUser>(elasticsearchUser,
                        x => x
                            .Id(updateUser.Id)
                            .Index("users")
                        );
                }
                else
                {
                    // Doc exists, update
                    var updateResponse = await elasticClient.UpdateAsync<ElasticsearchUser>(updateUser.Id,
                        x => x
                            .Index("users")
                            .Doc(elasticsearchUser)
                        );
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (userRepository.Get(id) == null)
                {
                    return NotFound();
                }
                throw;
            }
            return Ok(updateUser);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<User?>> DeleteUser(string? id)
        {
            User? user = await userRepository.DeleteUser(id);
            await elasticClient.DeleteAsync(new DeleteRequest(index: "users", user!.Id));
            return Ok();
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
                    await userFollowRepository.Remove(id, uid);
                    return Ok();
                }
                else
                {
                    try
                    {
                        await userFollowRepository.Add(id, uid);
                        return Ok();
                    }
                    catch (Exception)
                    {
                        return NotFound();
                    }
                }
            }
            return Forbid();
        }

        private async Task<Store> CreateStoreForUser(CreateStoreRequest request, User user)
        {
            // Add role to user
            user.HasRoles!.Add(new UserHasRole
            {
                UserId = user.Id,
                RoleId = (int)RoleEnum.StoreOwner,
            });

            // Update user information
            user.FullName = request.FullName;
            user.Gender = request.Gender;
            user.DateOfBirth = request.DateOfBirth!.Value.ToUniversalTime();
            user.Address = request.Address;
            user.CitizenshipNumber = request.CitizenshipNumber;
            user.CitizenshipDate = request.CitizenshipDate!.Value.ToUniversalTime();

            // Create store
            var store = new Store
            {
                UserId = user.Id,
                Name = request.Name,
                Description = request.Description,
                Address = request.StoreAddress,
                PhotoUrl = request.PhotoUrl,
                Rating = 0.0M,
                CreatedDate = DateTime.Now,
                Status = (int)StoreStatusEnum.Active,
            };

            var dynamicLinkResponse = await CreateStoreDynamicLink(store);
            store.ShareUrl = dynamicLinkResponse.ShortLink;

            // Save changes to database
            store = await storeRepository.CreateStoreForUser(user, store);
            return store;
        }

        /// <summary>
        /// Add user role to current user
        /// </summary>
        /// <returns></returns>
        [HttpPost("current/to-store")]
        [Authorize]
        public async Task<ActionResult> AddStoreRoleToCurrentUser(CreateStoreRequest request)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                // Check if user existed in database
                User? user = await userRepository.Get(uid);
                if (user != null)
                {
                    UserRecord? userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
                    var phoneNumber = userRecord.PhoneNumber;
                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        // Check if user already has store role
                        var roleExisted = user.HasRoles!.Any(x => x.RoleId == (int)RoleEnum.StoreOwner);
                        if (!roleExisted)
                        {
                            user.PhoneNumber = phoneNumber.Replace("+84", "0");
                            var store = await CreateStoreForUser(request, user);

                            var elasticsearchUser = new ElasticsearchUser
                            {
                                Id = user.Id,
                                Username = user.Username,
                                DisplayName = user.DisplayName,
                                Roles = user.HasRoles!.Select(x => (int)x.RoleId!).ToArray(),
                            };

                            var updateResponse = await elasticClient.UpdateAsync<ElasticsearchUser>(elasticsearchUser.Id,
                                x => x
                                    .Index("users")
                                    .Doc(elasticsearchUser)
                                );

                            var elasticsearchStore = new ElasticsearchStore
                            {
                                Id = store.Id,
                                Name = store.Name,
                                Rating = store.Rating,
                            };

                            var createResponse = await elasticClient.CreateAsync<ElasticsearchStore>(elasticsearchStore,
                                x => x
                                    .Id(store.Id)
                                    .Index("stores")
                                );

                            return Ok();
                        }
                    }
                }
                return BadRequest();
            }
            return Forbid();
        }

        [HttpGet("cccd/{cccd}")]
        [Authorize]
        public async Task<ActionResult> CheckIfCitizenshipNumberExisted(string cccd)
        {
            var isExisted = await userRepository.IsCitizenshipNumberExisted(cccd);
            if (isExisted) return Conflict();
            return Ok();
        }

        [HttpPost("staff")]
        [Authorize]
        public async Task<ActionResult<DetachedUser>> CreateStaff(CreateStaffUser createStaff)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (await UserRoleAuthorizer
                .AuthorizeUser(new RoleEnum[] { RoleEnum.Administrator, RoleEnum.Staff }, uid!, userRepository))
            {
                var email = createStaff.Email!.Trim();
                var hasRoles = new HashSet<UserHasRole>();
                // Check if user with email already exists
                var user = await userRepository.GetUserByEmail(email);
                if (user != null)
                {
                    if (user.HasRoles!.Any(x => x.RoleId == (int)RoleEnum.StoreOwner))
                    {
                        return Conflict();
                    }
                    hasRoles.Add(new UserHasRole
                    {
                        UserId = uid,
                        RoleId = (int)RoleEnum.Staff,
                    });
                    user.HasRoles = hasRoles;
                    await userRepository.UpdateStaff(user, user.Id!);
                    return Ok(await userRepository.GetDetached(user.Id!));
                }
                var placeholderId = Guid.NewGuid().ToString();
                // Set staff role
                hasRoles.Add(new UserHasRole
                {
                    UserId = placeholderId,
                    RoleId = (int)RoleEnum.Staff,
                });
                var newUser = new User
                {
                    Id = placeholderId,
                    DisplayName = email.Split("@").FirstOrDefault() ?? placeholderId,
                    Username = null,
                    Email = email,
                    HasRoles = hasRoles,
                    CreatedDate = DateTime.Now,
                    Status = (int)UserStatusEnum.Active,
                };

                await userRepository.Add(newUser);

                _ = Task.Run(() => SmtpMailSender.SendStaffMail(email));
                return Ok();
            }
            return Forbid();
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

                    // Check if user has shareUrl
                    if (string.IsNullOrWhiteSpace(user.ShareUrl))
                    {
                        // Create share_url for user
                        var baseUser = user.Adapt<User>();
                        var dynamicLinkResponse = await CreateUserDynamicLink(baseUser);
                        await userRepository.UpdateShareUrl(user.Id!, dynamicLinkResponse.ShortLink);
                        user.ShareUrl = dynamicLinkResponse.ShortLink;
                    }

                    // Check if user existed in Elasticsearch
                    var existsResponse = await elasticClient
                        .DocumentExistsAsync(new DocumentExistsRequest(index: "users", user.Id));
                    if (!existsResponse.Exists)
                    {
                        // Create user for Elasticsearch
                        var elasticsearchUser = new ElasticsearchUser
                        {
                            Id = user.Id,
                            Username = user.Username,
                            DisplayName = user.DisplayName,
                            Roles = user.HasRoles!.Select(x => (int)x.RoleId!).ToArray(),
                        };

                        var createResponse = await elasticClient.CreateAsync<ElasticsearchUser>(elasticsearchUser,
                            x => x
                                .Id(user.Id)
                                .Index("users")
                            );
                    }

                    // Check if user is deactivated
                    if (user.Status == (int)UserStatusEnum.Inactive)
                    {
                        var userItemId = ConvertUtility.ToGuid(user.Id!);
                        Console.WriteLine($"USERGUID: {userItemId}");
                        user.DeactivateReason = await deactivateReasonRepository.GetReasonByItemIdReadonly(userItemId);
                    }

                    // Check if store is deactivated
                    if (user.Store?.Status == (int)StoreStatusEnum.Inactive)
                    {
                        user.Store!.DeactivateReason = await deactivateReasonRepository.GetReasonByItemIdReadonly((Guid)user.Store.Id!);
                    }

                    // Return user with no collection attached (except roles)
                    return Ok(user);
                }
                else
                {
                    try
                    {
                        // Get user information from Firebase
                        UserRecord? userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
                        bool isUserRecordExisted = userRecord != null;

                        // Check if email already exists in database
                        var staff = await userRepository.GetReadonlyUserByEmail(userRecord!.Email);
                        if (isUserRecordExisted && staff != null)
                        {
                            // Staff account was created prior to login
                            // Update with information from Firebase
                            var hasRoles = new HashSet<UserHasRole>
                            {
                                new UserHasRole
                                {
                                    UserId = uid,
                                    RoleId = (int)RoleEnum.Staff,
                                }
                            };
                            var newStaff = new User
                            {
                                Id = uid,
                                DisplayName = staff.DisplayName,
                                Username = null,
                                Email = staff.Email,
                                PhotoUrl = userRecord.PhotoUrl,
                                HasRoles = hasRoles,
                                CreatedDate = staff.CreatedDate,
                                Status = staff.Status,
                            };

                            var dynamicLinkResponse = await CreateUserDynamicLink(newStaff);
                            newStaff.ShareUrl = dynamicLinkResponse.ShortLink;

                            await userRepository.UpdateStaff(newStaff, staff.Id!);

                            // Add user to Elasticsearch
                            await AddToElasticsearch(newStaff);
                            user = await userRepository.GetDetached(uid);
                            return Ok(user);
                        }
                        else
                        {
                            // User does not exist in database, creating user in database
                            // Default role to be Baker
                            var hasRoles = new HashSet<UserHasRole>
                            {
                                new UserHasRole
                                {
                                    UserId = uid,
                                    RoleId = (int)RoleEnum.Baker,
                                }
                            };
                            User newUser = new User
                            {
                                Id = uid,
                                DisplayName = isUserRecordExisted
                                ? userRecord!.DisplayName != null
                                ? userRecord!.DisplayName
                                : "Anonymous"
                                : "Anonymous",
                                Username = null,
                                Email = isUserRecordExisted ? userRecord!.Email : "Anonymous",
                                PhotoUrl = isUserRecordExisted ? userRecord!.PhotoUrl : null,
                                HasRoles = hasRoles,
                                CreatedDate = DateTime.Now,
                                Status = (int)UserStatusEnum.Active,
                            };

                            var dynamicLinkResponse = await CreateUserDynamicLink(newUser);
                            newUser.ShareUrl = dynamicLinkResponse.ShortLink;

                            await userRepository.Add(newUser);
                            // Add user to Elasticsearch
                            await AddToElasticsearch(newUser);
                        }
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

        private async Task AddToElasticsearch(User user)
        {
            var elasticsearchUser = new ElasticsearchUser
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Roles = user.HasRoles!.Select(x => (int)x.RoleId!).ToArray(),
            };

            var createResponse = await elasticClient.CreateAsync<ElasticsearchUser>(elasticsearchUser,
                x => x
                    .Id(user.Id)
                    .Index("users")
                );
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
            return Forbid();
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
            return Forbid();
        }

        [HttpPut("{id:length(1,128)}/profile")]
        [Authorize]
        public async Task<ActionResult> UpdateUserProfile(string id, UpdateProfileUser updateProfile)
        {
            // Get ID Token
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(id))
            {
                if (id == uid || await UserRoleAuthorizer
                .AuthorizeUser(new RoleEnum[] { RoleEnum.Administrator, RoleEnum.Staff }, uid!, userRepository))
                {
                    if (id == "current")
                    {
                        id = uid!;
                    }

                    try
                    {
                        await userRepository.UpdateUserProfile(id, updateProfile);
                        try
                        {
                            var user = await userRepository.GetDetached(id);
                            var temp = new User { Id = user!.Id, DisplayName = user.DisplayName, PhotoUrl = user.PhotoUrl };
                            var dynamicLinkResponse = await CreateUserDynamicLink(temp);
                            await userRepository.UpdateShareUrl(id, dynamicLinkResponse.ShortLink);

                            var elasticsearchUser = new ElasticsearchUser
                            {
                                Id = user.Id,
                                Username = user.Username,
                                DisplayName = user.DisplayName,
                                Roles = user.HasRoles!.Select(x => (int)x.RoleId!).ToArray(),
                            };

                            // Does doc exist on Elasticsearch?
                            var existsResponse = await elasticClient.DocumentExistsAsync(new DocumentExistsRequest(index: "users", user.Id));
                            if (!existsResponse.Exists)
                            {
                                // Doc doesn't exist, create new
                                var createResponse = await elasticClient.CreateAsync<ElasticsearchUser>(elasticsearchUser,
                                    x => x
                                        .Id(user.Id)
                                        .Index("users")
                                    );
                            }
                            else
                            {
                                // Doc exists, update
                                var updateResponse = await elasticClient.UpdateAsync<ElasticsearchUser>(user.Id,
                                    x => x
                                        .Index("users")
                                        .Doc(elasticsearchUser)
                                    );
                            }
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }
                        return Ok();
                    }
                    catch (UsernameTakenException)
                    {
                        return Conflict();
                    }
                }
            }
            return Forbid();
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
            return Forbid();
        }

        [HttpGet("{id:length(1,128)}/orders")]
        [Authorize]
        public async Task<ActionResult<InfoOrderPage>> GetOrdersOfUser(
            string id,
            [FromQuery] int[] status,
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
                    var orderPage = new InfoOrderPage();
                    orderPage.TotalPages = (int)Math.Ceiling((decimal)await orderRepository.CountOrdersOfUser(id, status) / take);
                    orderPage.Orders = orderRepository.GetOrdersOfUser(id, status, (page - 1) * take, take);
                    return Ok(orderPage);
                }
                return BadRequest();
            }
            return Forbid();
        }

        [HttpGet("{id:length(1,128)}/notifications")]
        [Authorize]
        public async Task<ActionResult<DetailNotificationPage>> GetNotificationsOfUser(
            string id,
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    if (id == "current")
                    {
                        id = uid;
                    }
                    var notificationPage = new DetailNotificationPage();
                    notificationPage.UnreadCount = await notificationRepository.CountUnreadOfUser(id);
                    notificationPage.Notifications = notificationRepository.GetNotificationsOfUser(id, (page - 1) * take, take);
                    return Ok(notificationPage);
                }
                return BadRequest();
            }
            return Forbid();
        }

        [HttpGet("{id:length(1,128)}/addresses")]
        [Authorize]
        public ActionResult<OrderAddressPage> GetAddressesOfUser(
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
                    var addressPage = new OrderAddressPage();
                    addressPage.Addresses = orderRepository.GetAddressesOfUser(id, (page - 1) * take, take);
                    return Ok(addressPage);
                }
                return BadRequest();
            }
            return Forbid();
        }

        private async Task CheckAndAddDevice(string? FcmToken, string uid)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(FcmToken))
                {
                    // Check if device existed in database
                    var userDevice = await userDeviceRepository.GetReadonly(FcmToken);
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

        private async Task<CreateShortDynamicLinkResponse> CreateUserDynamicLink(User user)
        {
            return await DynamicLinkHelper.CreateDynamicLink(
                path: "profile",
                linkService: firebaseDynamicLinksService,
                id: user.Id!,
                name: user.DisplayName!,
                description: "",
                photoUrl: user.PhotoUrl ?? "",
                thumbnailUrl: null
            );
        }

        private async Task<CreateShortDynamicLinkResponse> CreateStoreDynamicLink(Store store)
        {
            return await DynamicLinkHelper.CreateDynamicLink(
                path: "store-details",
                linkService: firebaseDynamicLinksService,
                id: store.Id.ToString()!,
                name: store.Name!,
                description: store.Description ?? "",
                photoUrl: store.PhotoUrl ?? "",
                thumbnailUrl: null
            );
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<FollowAwareSimpleUserPage>> SearchUsers(
            [FromQuery] string? query,
            [FromQuery] int[]? roles,
            [FromQuery] string? lastId,
            [FromQuery] double? lastScore,
            [Range(1, int.MaxValue)] int take = 5)
        {
            var searchDescriptor = new SearchDescriptor<ElasticsearchUser>();
            var descriptor = new QueryContainerDescriptor<ElasticsearchUser>();
            var shouldContainer = new List<QueryContainer>();
            var filterContainer = new List<QueryContainer>();

            if (string.IsNullOrWhiteSpace(query)
                && (roles == null || roles.Length == 0))
            {
                var emptyPage = new SimpleUserPage();
                emptyPage.TotalPages = 0;
                emptyPage.Users = new List<SimpleUser>();

                return Ok(emptyPage);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                shouldContainer.Add(descriptor
                    .Match(m => m
                        .Field(f => f.Username)
                        .Query(query)
                        .Fuzziness(Fuzziness.EditDistance(2))
                    )
                );

                shouldContainer.Add(descriptor
                    .Match(m => m
                        .Field(f => f.DisplayName)
                        .Query(query)
                        .Fuzziness(Fuzziness.EditDistance(2))
                    )
                );
            }

            if (roles != null)
            {
                shouldContainer.Add(descriptor
                    .Terms(x => x
                        .Field(f => f.Roles)
                        .Terms(roles)
                    )
                );

                filterContainer.Add(descriptor
                    .Terms(x => x
                        .Field(f => f.Roles)
                        .Terms(roles)
                    )
                );
            }

            if (lastId != null && lastScore != null)
            {
                searchDescriptor = searchDescriptor.SearchAfter(lastScore, lastId);
            }

            searchDescriptor = searchDescriptor.Index("users")
                .Size(take)
                .MinScore(0.01D)
                .Sort(ss => ss
                    .Descending(SortSpecialField.Score)
                    .Descending(f => f.Id.Suffix("keyword"))
                )
                .Query(q => q
                    .Bool(b => b
                        .Should(shouldContainer.ToArray())
                        .Filter(filterContainer.ToArray())
                    )
                );

            var searchResponse = await elasticClient.SearchAsync<ElasticsearchUser>(searchDescriptor);

            var elasticsearchUsers = new List<KeyValuePair<string, double>>();

            foreach (var hit in searchResponse.Hits)
            {
                elasticsearchUsers.Add(
                    new KeyValuePair<string, double>(hit.Source.Id!, (double)hit.Score!)
                );
            }

            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userIds = elasticsearchUsers.Select(x => x.Key).ToList();
            var suggestedUsers = await userRepository.GetSuggestedUsers(userIds, uid ?? "");

            var users = elasticsearchUsers
                .Join(suggestedUsers, es => es.Key, us => us.Id!, (es, us) => { us.Score = es.Value; return us; });

            var userPage = new FollowAwareSimpleUserPage();
            userPage.Users = users;

            return Ok(userPage);
        }
    }
}
