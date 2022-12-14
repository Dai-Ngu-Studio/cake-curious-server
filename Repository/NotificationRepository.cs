using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Notifications;
using Repository.Interfaces;
using Repository.Models.Notifications;

namespace Repository
{
    public class NotificationRepository : INotificationRepository
    {
        public async Task CreateNotificationContent(NotificationContent content)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = db.Database.BeginTransactionAsync())
            {
                db.NotificationContents.Add(content);
                await db.SaveChangesAsync();
                await db.Database.CommitTransactionAsync();
            }
        }

        public async Task<int> CountUnreadOfUser(string uid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == uid)
                .Where(x => x.Status == (int)NotificationStatusEnum.Unread)
                .CountAsync();
        }

        public IEnumerable<DetailNotifcation> GetNotificationsOfUser(string uid, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Notifications
                .AsNoTracking()
                .OrderByDescending(x => x.Content!.NotificationDate)
                .Where(x => x.UserId == uid)
                .Skip(skip)
                .Take(take)
                .ProjectToType<DetailNotifcation>();
        }

        public async Task SwitchNotificationStatus(Guid id)
        {
            var db = new CakeCuriousDbContext();
            string query = "update [Notification] set [Notification].[status] = abs([Notification].[status] - 1) where [Notification].[id] = {0}";
            await db.Database.ExecuteSqlRawAsync(query, id);
            await db.SaveChangesAsync();
        }

        public async Task UpdateRangeNotificationStatus(List<Guid> ids, int status)
        {
            var db = new CakeCuriousDbContext();
            var query = "update [Notification] set [Notification].[status] = {0} where [Notification].[id] in ";
            var formattedIds = ids.Select(x => $"'{x}'");
            query += $"({string.Join(",", formattedIds)})";
            await db.Database.ExecuteSqlRawAsync(query, status);
            await db.SaveChangesAsync();
        }
    }
}
