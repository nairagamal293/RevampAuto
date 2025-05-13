using Microsoft.EntityFrameworkCore;
using RevampAuto.Data;
using RevampAuto.DTOs;
using RevampAuto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevampAuto.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDiscountService _discountService;

        public OrderService(ApplicationDbContext context, IDiscountService discountService)
        {
            _context = context;
            _discountService = discountService;
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Images)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Price = oi.UnitPrice,
                        Quantity = oi.Quantity,
                        ImageUrl = oi.Product.Images.FirstOrDefault(i => i.IsMainImage).ImagePath
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<OrderDto> GetOrderByIdAsync(int id, string userId)
        {
            return await _context.Orders
                .Where(o => o.Id == id && o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Images)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Price = oi.UnitPrice,
                        Quantity = oi.Quantity,
                        ImageUrl = oi.Product.Images.FirstOrDefault(i => i.IsMainImage).ImagePath
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get cart items
                var cartItems = await _context.CartItems
                    .Where(ci => ci.Cart.UserId == userId)
                    .Include(ci => ci.Product)
                    .ToListAsync();

                if (!cartItems.Any())
                    throw new Exception("Cart is empty");

                // Calculate total
                decimal discountAmount = 0;
                decimal subtotal = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);

                // Apply discount if provided
                if (!string.IsNullOrEmpty(dto.DiscountCode))
                {
                    var discountResult = await _discountService.ApplyDiscountAsync(dto.DiscountCode);
                    if (discountResult.IsValid)
                    {
                        discountAmount = subtotal * (discountResult.DiscountPercentage / 100);
                    }
                }

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = subtotal - discountAmount,
                    ShippingAddress = dto.ShippingAddress,
                    Status = "Pending"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Add order items
                var orderItems = cartItems.Select(ci => new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product.Price
                }).ToList();

                _context.OrderItems.AddRange(orderItems);

                // Clear cart
                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetOrderByIdAsync(order.Id, userId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Images)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    UserEmail = o.User.Email,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Price = oi.UnitPrice,
                        Quantity = oi.Quantity,
                        ImageUrl = oi.Product.Images.FirstOrDefault(i => i.IsMainImage).ImagePath
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return false;

            order.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}