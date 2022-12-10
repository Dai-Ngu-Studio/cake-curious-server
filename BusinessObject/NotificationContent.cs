using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("NotificationContent")]
    public class NotificationContent
    {
        public NotificationContent()
        {
            Notifications = new HashSet<Notification>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("item_id")]
        public Guid? ItemId { get; set; }

        [Column("item_type")]
        public int? ItemType { get; set; }

        [Column("item_name", TypeName = "nvarchar(256)")]
        public string? ItemName { get; set; }

        [Column("notification_type")]
        public int? NotificationType { get; set; }

        [Column("notification_date", TypeName = "datetime2(7)")]
        public DateTime? NotificationDate { get; set; }

        [Column("title", TypeName = "nvarchar(256)")]
        public string? Title { get; set; }

        [Column("en_title", TypeName = "nvarchar(256)")]
        public string? EnTitle { get; set; }

        [Column("content", TypeName = "nvarchar(512)")]
        public string? Content { get; set; }

        [Column("en_content", TypeName = "nvarchar(512)")]
        public string? EnContent { get; set; }

        [InverseProperty("Content")]
        public ICollection<Notification>? Notifications { get; set; }
    }
}