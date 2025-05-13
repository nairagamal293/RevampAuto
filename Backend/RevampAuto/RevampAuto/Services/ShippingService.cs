using Microsoft.EntityFrameworkCore;
using RevampAuto.Data;
using RevampAuto.DTOs;
using RevampAuto.Models;
using System.Threading.Tasks;

namespace RevampAuto.Services
{
    public class ShippingService : IShippingService
    {
        private readonly ApplicationDbContext _context;

        public ShippingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ShippingDetailsDto> GetShippingDetailsAsync(int orderId, string userId)
        {
            // Verify user owns the order
            if (!await _context.Orders.AnyAsync(o => o.Id == orderId && o.UserId == userId))
                return null;

            return await _context.ShippingDetails
                .Where(s => s.OrderId == orderId)
                .Select(s => new ShippingDetailsDto
                {
                    FullName = s.FullName,
                    AddressLine1 = s.AddressLine1,
                    AddressLine2 = s.AddressLine2,
                    City = s.City,
                    State = s.State,
                    PostalCode = s.PostalCode,
                    Country = s.Country,
                    PhoneNumber = s.PhoneNumber,
                    TrackingNumber = s.TrackingNumber,
                    ShippingMethod = s.ShippingMethod,
                    ShippedDate = s.ShippedDate,
                    EstimatedDeliveryDate = s.EstimatedDeliveryDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CreateShippingDetailsAsync(int orderId, string userId, ShippingDetailsCreateDto dto)
        {
            // Verify user owns the order
            if (!await _context.Orders.AnyAsync(o => o.Id == orderId && o.UserId == userId))
                return false;

            // Check if shipping details already exist
            if (await _context.ShippingDetails.AnyAsync(s => s.OrderId == orderId))
                return false;

            var shippingDetails = new ShippingDetails
            {
                OrderId = orderId,
                FullName = dto.FullName,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country,
                PhoneNumber = dto.PhoneNumber
            };

            _context.ShippingDetails.Add(shippingDetails);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateOrderShippingStatusAsync(int orderId, UpdateOrderStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            order.Status = dto.Status;

            // If shipping, update shipping details
            if (dto.Status == "Shipped" && !string.IsNullOrEmpty(dto.TrackingNumber))
            {
                var shippingDetails = await _context.ShippingDetails
                    .FirstOrDefaultAsync(s => s.OrderId == orderId);

                if (shippingDetails != null)
                {
                    shippingDetails.TrackingNumber = dto.TrackingNumber;
                    shippingDetails.ShippedDate = DateTime.UtcNow;
                    shippingDetails.EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3); // Example: 3 days from now
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}