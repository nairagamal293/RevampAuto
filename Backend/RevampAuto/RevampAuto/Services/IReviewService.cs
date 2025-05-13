using RevampAuto.DTOs;

namespace RevampAuto.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId);
        Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewDto dto);
        Task<bool> UpdateReviewAsync(int id, string userId, UpdateReviewDto dto);
        Task<bool> DeleteReviewAsync(int id, string userId);
    }
}
