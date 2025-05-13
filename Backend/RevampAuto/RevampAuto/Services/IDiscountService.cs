using RevampAuto.DTOs;

namespace RevampAuto.Services
{
    public interface IDiscountService
    {
        Task<IEnumerable<DiscountDto>> GetAllDiscountsAsync();
        Task<DiscountDto> GetDiscountByIdAsync(int id);
        Task<DiscountDto> CreateDiscountAsync(CreateDiscountDto dto);
        Task<bool> UpdateDiscountAsync(int id, UpdateDiscountDto dto);
        Task<bool> DeleteDiscountAsync(int id);
        Task<DiscountApplicationResult> ApplyDiscountAsync(string code);
    }

    public class DiscountApplicationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}
