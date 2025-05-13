using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevampAuto.DTOs;
using RevampAuto.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RevampAuto.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
            return Ok(favorites);
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorites([FromBody] AddToFavoritesDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _favoriteService.AddToFavoritesAsync(userId, dto.ProductId);

            if (!result)
                return BadRequest("Product is already in favorites or doesn't exist");

            return CreatedAtAction(nameof(GetUserFavorites), null);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromFavorites(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _favoriteService.RemoveFromFavoritesAsync(userId, productId);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}