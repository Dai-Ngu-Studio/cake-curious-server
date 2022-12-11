using BusinessObject;
using Repository.Interfaces;

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
    }
}
