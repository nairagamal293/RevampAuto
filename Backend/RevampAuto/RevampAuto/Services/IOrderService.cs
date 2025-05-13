using RevampAuto.DTOs;

namespace RevampAuto.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
        Task<OrderDto> GetOrderByIdAsync(int id, string userId);
        Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto dto);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<bool> UpdateOrderStatusAsync(int id, string status);
    }
}
