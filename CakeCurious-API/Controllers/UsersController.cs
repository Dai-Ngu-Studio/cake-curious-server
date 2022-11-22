using BusinessObject;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using Repository.Constants.Orders;
using Repository.Constants.Products;
using Repository.Constants.Roles;
using Repository.Constants.Users;
using Repository.Interfaces;
using Repository.Models.Orders;
using Repository.Models.Recipes;
using Repository.Models.Roles;
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
        private readonly IElasticClient elasticClient;

        public UsersController(IUserRepository _userRepository, IUserDeviceRepository _userDeviceRepository,
            IUserFollowRepository _userFollowRepository, IRecipeRepository _recipeRepository, IStoreRepository _storeRepository, IOrderRepository _orderRepository, IElasticClient _elasticClient)
        {
            userRepository = _userRepository;
            userDeviceRepository = _userDeviceRepository;
            userFollowRepository = _userFollowRepository;
            recipeRepository = _recipeRepository;
            storeRepository = _storeRepository;
            orderRepository = _orderRepository;
            elasticClient = _elasticClient;
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
                };
                try
                {
                    foreach (int role in newUser!.Roles!)
                    {
                        updateUser.HasRoles!.Add(new UserHasRole
                        {
                            UserId = newUser.Id,
                            RoleId = role,
                        });
                    }
                    // Add role to user
                }
                catch (Exception)
                {
                    return BadRequest();
                }
                await userRepository.Update(updateUser);

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
            user.DateOfBirth = request.DateOfBirth;
            user.Address = request.Address;
            user.CitizenshipNumber = request.CitizenshipNumber;
            user.CitizenshipDate = request.CitizenshipDate;

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
                        // Add user to Elasticsearch
                        var elasticsearchUser = new ElasticsearchUser
                        {
                            Id = newUser.Id,
                            Username = newUser.Username,
                            DisplayName = newUser.DisplayName,
                            Roles = newUser.HasRoles.Select(x => (int)x.RoleId!).ToArray(),
                        };

                        var createResponse = await elasticClient.CreateAsync<ElasticsearchUser>(elasticsearchUser,
                            x => x
                                .Id(newUser.Id)
                                .Index("users")
                            );
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

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<SimpleUserPage>> SearchUsers(
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
                    .MultiMatch(m => m
                        .Fields(f => f
                            .Field(ff => ff.DisplayName)
                            .Field(ff => ff.Username))
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

            var userIds = elasticsearchUsers.Select(x => x.Key).ToList();
            var suggestedUsers = await userRepository.GetSuggestedUsers(userIds);

            var users = elasticsearchUsers
                .Join(suggestedUsers, es => es.Key, us => us.Id!, (es, us) => { us.Score = es.Value; return us; });

            var userPage = new SimpleUserPage();
            userPage.Users = users;

            return Ok(userPage);
        }
    }
}
