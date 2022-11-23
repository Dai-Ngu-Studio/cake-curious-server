﻿using BusinessObject;
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
        public async Task<IEnumerable<AdminDashboardUser>?> GetList(string? search, string? order_by, string? filter, int page, int size)
        {
            try
            {
                var db = new CakeCuriousDbContext();
                IEnumerable<User> users = await db.Users!.Include(u => u.HasRoles)!.ThenInclude(hr => hr.Role).ToListAsync();
                IEnumerable<AdminDashboardUser> result;
                //Search
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
                result = users.Adapt<IEnumerable<AdminDashboardUser>>().Skip((page - 1) * size)
                            .Take(size).ToList();
                foreach (var r in result)
                {
                    foreach (var hasRole in users!.FirstOrDefault(u => u.Id == r.Id)!.HasRoles!)
                    {
                        r!.Roles!.Add(hasRole!.Role!.Id!.Value);
                    }
                }
                return result;
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

        public async Task<int> CountFollowingOfUser(string uid)
        {
            var db = new CakeCuriousDbContext();
            return await db.UserFollows
                .Where(x => x.FollowerId == uid)
                .CountAsync();
        }

        public async Task<ICollection<FollowingUser>> GetFollowingOfUser(string uid, string currentUserId, int skip, int take)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("userId", currentUserId);
                var db = new CakeCuriousDbContext();
                return await db.UserFollows
                    .Where(x => x.FollowerId == uid)
                    .Skip(skip)
                    .Take(take)
                    .ProjectToType<FollowingUser>()
                    .ToListAsync();
            }
        }

        public async Task<int> CountFollowersOfUser(string uid)
        {
            var db = new CakeCuriousDbContext();
            return await db.UserFollows
                .Where(x => x.UserId == uid)
                .CountAsync();
        }

        public async Task<ICollection<FollowerUser>> GetFollowersOfUser(string uid, string currentUserId, int skip, int take)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("userId", currentUserId);
                var db = new CakeCuriousDbContext();
                return await db.UserFollows
                    .Where(x => x.UserId == uid)
                    .Skip(skip)
                    .Take(take)
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
            if (user == null) return null;
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
            //Remove all role of current update user
            IEnumerable<UserHasRole> uhr = db.UserHasRoles.Where(uhr => uhr.UserId == obj.Id);
            if (uhr.Count() > 0) db.UserHasRoles.RemoveRange(uhr);
            db.Users.Update(obj);
            await db.SaveChangesAsync();
        }

        public async Task<User?> DeleteUser(string? id)
        {
            User? user = null;
            try
            {
                user = await Get(id!);
                if (user == null) throw new Exception("User that need to delete does not exist");
                user.Status = (int)UserStatusEnum.Inactive;
                var db = new CakeCuriousDbContext();
                db.Entry<User>(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await db.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<ProfileUser?> GetProfileUser(string? id, string currentUserId)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("userId", currentUserId);
                var db = new CakeCuriousDbContext();
                return await db.Users.Where(x => x.Id == id).ProjectToType<ProfileUser>().FirstOrDefaultAsync();
            }
        }

        public async Task<ICollection<SimpleUser>> GetSuggestedUsers(List<string> userIds)
        {
            var db = new CakeCuriousDbContext();
            return await db.Users
                .AsNoTracking()
                .Where(x => userIds.Any(y => y == x.Id))
                .Where(x => x.Status == (int)UserStatusEnum.Active)
                .ProjectToType<SimpleUser>()
                .ToListAsync();
        }

        public async Task<int> UpdateShareUrl(string id, string shareUrl)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                string query = "update [User] set [User].[share_url] = {0} where [User].[id] = {1}";
                var rows = await db.Database.ExecuteSqlRawAsync(query, shareUrl, id);
                await transaction.CommitAsync();
                return rows;
            }
        }
    }
}
