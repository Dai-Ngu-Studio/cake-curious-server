using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Users;
using Repository.Interfaces;
using Repository.Models.Users;
using System.Collections.Generic;

namespace Repository
{
    public class UserRepository : IUserRepository
    {
        public IEnumerable<User> searchUserByDisplayName(string key, IEnumerable<User> users)
        {
            return users.Where(x => x!.DisplayName!.Contains(key)).ToList();
        }
        public IEnumerable<User> filterByInactiveStatus(IEnumerable<User> users)
        {
            return users.Where(x => x.Status == (int)UserStatusEnum.Inactive).ToList();
        }
        public IEnumerable<User> filterByActiveStatus(IEnumerable<User> users)
        {
            return users.Where(x => x.Status == (int)UserStatusEnum.Active).ToList();
        }
        public IEnumerable<User> orderByDescDisplayName(IEnumerable<User> users)
        {
            return users.OrderByDescending(u => u.DisplayName).ToList();
        }
        public IEnumerable<User> orderByAscDisplayName(IEnumerable<User> users)
        {
            return users.OrderBy(u => u.DisplayName).ToList();
        }
        public IEnumerable<AdminDashboardUser>? GetList(string? search, string? order_by, string? filter, int page, int size)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<User> users = db.Users.ToList();
            try
            {   //Search
                if (search != null)
                {
                    users = searchUserByDisplayName(search, users);
                }
                //Filter
                if (filter != null && filter == UserStatusEnum.Active.ToString())
                {
                    users = filterByActiveStatus(users);
                }
                else if (filter != null && filter == UserStatusEnum.Inactive.ToString())
                {
                    users = filterByInactiveStatus(users);
                }
                //Orderby
                if (order_by != null && order_by == UserSortEnum.AscDisplayName.ToString())
                {
                    users = orderByAscDisplayName(users);
                }
                else if (order_by != null && order_by == UserSortEnum.DescDisplayName.ToString())
                {
                    users = orderByDescDisplayName(users);
                }
                return users.Adapt<IEnumerable<AdminDashboardUser>>().Skip((page - 1) * size)
                            .Take(size).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        public int CountDashboardUser(string? search, string? order_by, string? filter)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<User> users = db.Users.ToList();
            try
            {   //Search
                if (search != null)
                {
                    users = searchUserByDisplayName(search, users);
                }
                //Filter
                if (filter != null && filter == UserStatusEnum.Active.ToString())
                {
                    users = filterByActiveStatus(users);
                }
                else if (filter != null && filter == UserStatusEnum.Inactive.ToString())
                {
                    users = filterByInactiveStatus(users);
                }
                //Orderby
                if (order_by != null && order_by == UserSortEnum.AscDisplayName.ToString())
                {
                    users = orderByAscDisplayName(users);
                }
                else if (order_by != null && order_by == UserSortEnum.DescDisplayName.ToString())
                {
                    users = orderByDescDisplayName(users);
                }
                return users.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }
        public async Task<ICollection<FollowingUser>> GetFollowingOfUser(string uid, string currentUserId)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("userId", currentUserId);

                var db = new CakeCuriousDbContext();
                return await db.UserFollows
                    .Where(x => x.FollowerId == uid)
                    .ProjectToType<FollowingUser>()
                    .ToListAsync();
            }
        }

        public async Task<ICollection<FollowerUser>> GetFollowersOfUser(string uid, string currentUserId)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("userId", currentUserId);
                var db = new CakeCuriousDbContext();
                return await db.UserFollows
                    .Where(x => x.UserId == uid)
                    .ProjectToType<FollowerUser>()
                    .ToListAsync();
            }
        }

        public async Task<User?> Get(string uid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Users.Include(x => x.HasRoles).FirstOrDefaultAsync(x => x.Id == uid);
        }
        public async Task<UserDetailForWeb?> GetUserDetailForWeb(string uid)
        {
            var db = new CakeCuriousDbContext();
            List<string> rolestring = new List<string>();
            UserDetailForWeb? user = await db.Users.ProjectToType<UserDetailForWeb>().FirstOrDefaultAsync(x => x.Id == uid);
            IEnumerable<UserHasRole> roles = await db.UserHasRoles.Include(ur => ur.Role).ToListAsync();
            foreach (var role in roles)
            {
                if (role.UserId == user!.Id)
                {
                    rolestring.Add(role!.Role!.Name!);
                }
            }
            user!.Roles = rolestring;
            return user;
        }

        public async Task<DetachedUser?> GetDetached(string uid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Users.Where(x => x.Id == uid).ProjectToType<DetachedUser>().FirstOrDefaultAsync();
        }

        public async Task Add(User obj)
        {
            var db = new CakeCuriousDbContext();
            await db.Users.AddAsync(obj);
            await db.SaveChangesAsync();
        }

        public async Task Update(User obj)
        {
            var db = new CakeCuriousDbContext();
            db.Users.Update(obj);
            await db.SaveChangesAsync();
        }
    }
}
