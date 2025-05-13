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
    public class ShippingController : ControllerBase
    {
        private readonly IShippingService _shippingService;

        public ShippingController(IShippingService shippingService)
        {
            _shippingService = shippingService;
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetShippingDetails(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var shippingDetails = await _shippingService.GetShippingDetailsAsync(orderId, userId);

            if (shippingDetails == null)
                return NotFound();

            return Ok(shippingDetails);
        }

        [HttpPost("order/{orderId}")]
        public async Task<IActionResult> CreateShippingDetails(int orderId, [FromBody] ShippingDetailsCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _shippingService.CreateShippingDetailsAsync(orderId, userId, dto);

            if (!result)
                return BadRequest("Unable to create shipping details");

            return CreatedAtAction(nameof(GetShippingDetails), new { orderId }, null);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("order/{orderId}/update-status")]
        public async Task<IActionResult> UpdateOrderShippingStatus(int orderId, [FromBody] UpdateOrderStatusDto dto)
        {
            var result = await _shippingService.UpdateOrderShippingStatusAsync(orderId, dto);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}