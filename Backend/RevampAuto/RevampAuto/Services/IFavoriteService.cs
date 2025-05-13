using RevampAuto.DTOs;

namespace RevampAuto.Services
{
    public interface IFavoriteService
    {
        Task<IEnumerable<FavoriteDto>> GetUserFavoritesAsync(string userId);
        Task<bool> AddToFavoritesAsync(string userId, int productId);
        Task<bool> RemoveFromFavoritesAsync(string userId, int productId);
    }
}
