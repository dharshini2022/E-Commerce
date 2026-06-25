namespace Ecommerce.Models.DTOs
{
    public class RecentNotificationDTO
    {
        public int Id { get; set; }
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!; // "info" | "warning" | "success" | "error"
        public DateTime NotifiedAt { get; set; }
        public string TimeAgo { get; set; } = null!;
        public bool Read { get; set; }
    }
}