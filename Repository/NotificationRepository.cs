using BusinessObject;
using Mapster;
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

        public IEnumerable<DetailNotifcation> GetNotificationsOfUser(string uid, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Notifications
                .OrderByDescending(x => x.Content!.NotificationDate)
                .Where(x => x.UserId == uid)
                .Skip(skip)
                .Take(take)
                .ProjectToType<DetailNotifcation>();
        }
    }
}
