using RevampAuto.DTOs;

namespace RevampAuto.Services
{
    public interface IContactService
    {
        Task CreateContactMessageAsync(ContactMessageDto dto);
        Task<IEnumerable<ContactMessageDto>> GetAllMessagesAsync();
        Task<IEnumerable<ContactMessageDto>> GetUnreadMessagesAsync();
        Task<bool> MarkMessageAsReadAsync(int id);
    }
}
