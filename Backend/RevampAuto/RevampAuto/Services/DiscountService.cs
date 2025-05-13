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
    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _context;

        public DiscountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DiscountDto>> GetAllDiscountsAsync()
        {
            return await _context.Discounts
                .Include(d => d.Category)
                .Include(d => d.Product)
                .Select(d => new DiscountDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    Description = d.Description,
                    Percentage = d.Percentage,
                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    MaxUses = d.MaxUses,
                    CurrentUses = d.CurrentUses,
                    IsActive = d.IsActive,
                    CategoryId = d.CategoryId,
                    CategoryName = d.Category != null ? d.Category.Name : null,
                    ProductId = d.ProductId,
                    ProductName = d.Product != null ? d.Product.Name : null
                })
                .ToListAsync();
        }

        public async Task<DiscountDto> GetDiscountByIdAsync(int id)
        {
            return await _context.Discounts
                .Include(d => d.Category)
                .Include(d => d.Product)
                .Where(d => d.Id == id)
                .Select(d => new DiscountDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    Description = d.Description,
                    Percentage = d.Percentage,
                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    MaxUses = d.MaxUses,
                    CurrentUses = d.CurrentUses,
                    IsActive = d.IsActive,
                    CategoryId = d.CategoryId,
                    CategoryName = d.Category != null ? d.Category.Name : null,
                    ProductId = d.ProductId,
                    ProductName = d.Product != null ? d.Product.Name : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DiscountDto> CreateDiscountAsync(CreateDiscountDto dto)
        {
            var discount = new Discount
            {
                Code = dto.Code,
                Description = dto.Description,
                Percentage = dto.Percentage,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MaxUses = dto.MaxUses,
                CurrentUses = 0,
                IsActive = true,
                CategoryId = dto.CategoryId,
                ProductId = dto.ProductId
            };

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            return await GetDiscountByIdAsync(discount.Id);
        }

        public async Task<bool> UpdateDiscountAsync(int id, UpdateDiscountDto dto)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return false;

            discount.Description = dto.Description;
            discount.EndDate = dto.EndDate;
            discount.MaxUses = dto.MaxUses;
            discount.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDiscountAsync(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return false;

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DiscountApplicationResult> ApplyDiscountAsync(string code)
        {
            var discount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.Code == code &&
                                         d.IsActive &&
                                         DateTime.UtcNow >= d.StartDate &&
                                         DateTime.UtcNow <= d.EndDate &&
                                         (d.MaxUses == null || d.CurrentUses < d.MaxUses));

            if (discount == null)
                return new DiscountApplicationResult
                {
                    IsValid = false,
                    Message = "Invalid or expired discount code"
                };

            discount.CurrentUses++;
            await _context.SaveChangesAsync();

            return new DiscountApplicationResult
            {
                IsValid = true,
                Message = "Discount applied successfully",
                DiscountPercentage = discount.Percentage
            };
        }
    }
}