using Microsoft.EntityFrameworkCore;
using RevampAuto.Data;
using RevampAuto.DTOs;
using RevampAuto.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevampAuto.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ApplicationDbContext _context;

        public FavoriteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FavoriteDto>> GetUserFavoritesAsync(string userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                .ThenInclude(p => p.Images)
                .Select(f => new FavoriteDto
                {
                    Id = f.Id,
                    ProductId = f.ProductId,
                    ProductName = f.Product.Name,
                    ProductPrice = f.Product.Price,
                    ProductImage = f.Product.Images.FirstOrDefault(i => i.IsMainImage).ImagePath,
                    AddedAt = f.AddedAt
                })
                .ToListAsync();
        }

        public async Task<bool> AddToFavoritesAsync(string userId, int productId)
        {
            // Check if already exists
            if (await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId))
                return false;

            // Check if product exists
            if (!await _context.Products.AnyAsync(p => p.Id == productId))
                return false;

            var favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromFavoritesAsync(string userId, int productId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (favorite == null)
                return false;

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}