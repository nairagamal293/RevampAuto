using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevampAuto.DTOs;
using RevampAuto.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RevampAuto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var reviews = await _reviewService.GetProductReviewsAsync(productId);
            return Ok(reviews);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var review = await _reviewService.CreateReviewAsync(userId, dto);
            return CreatedAtAction(nameof(GetProductReviews), new { productId = review.ProductId }, review);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _reviewService.UpdateReviewAsync(id, userId, dto);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _reviewService.DeleteReviewAsync(id, userId);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}