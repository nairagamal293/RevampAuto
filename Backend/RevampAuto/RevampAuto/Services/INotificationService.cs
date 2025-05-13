using RevampAuto.DTOs;

namespace RevampAuto.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);
        Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(string userId);
        Task<bool> MarkAsReadAsync(int id, string userId, bool isRead);
        Task<bool> DeleteNotificationAsync(int id, string userId);
    }
}
