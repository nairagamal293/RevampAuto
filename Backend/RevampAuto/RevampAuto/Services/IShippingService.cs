using RevampAuto.DTOs;

namespace RevampAuto.Services
{
    public interface IShippingService
    {
        Task<ShippingDetailsDto> GetShippingDetailsAsync(int orderId, string userId);
        Task<bool> CreateShippingDetailsAsync(int orderId, string userId, ShippingDetailsCreateDto dto);
        Task<bool> UpdateOrderShippingStatusAsync(int orderId, UpdateOrderStatusDto dto);
    }
}
