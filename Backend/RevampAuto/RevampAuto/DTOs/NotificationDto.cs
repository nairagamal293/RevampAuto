namespace RevampAuto.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string NotificationType { get; set; }
        public int? RelatedEntityId { get; set; }
    }

    public class CreateNotificationDto
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
        public int? RelatedEntityId { get; set; }
    }

    public class MarkAsReadDto
    {
        public bool IsRead { get; set; }
    }
}
