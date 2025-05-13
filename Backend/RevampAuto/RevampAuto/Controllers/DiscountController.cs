using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevampAuto.DTOs;
using RevampAuto.Services;
using System.Threading.Tasks;

namespace RevampAuto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDiscounts()
        {
            var discounts = await _discountService.GetAllDiscountsAsync();
            return Ok(discounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDiscountById(int id)
        {
            var discount = await _discountService.GetDiscountByIdAsync(id);
            if (discount == null)
                return NotFound();

            return Ok(discount);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountDto dto)
        {
            var discount = await _discountService.CreateDiscountAsync(dto);
            return CreatedAtAction(nameof(GetDiscountById), new { id = discount.Id }, discount);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDiscount(int id, [FromBody] UpdateDiscountDto dto)
        {
            var result = await _discountService.UpdateDiscountAsync(id, dto);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            var result = await _discountService.DeleteDiscountAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyDiscount([FromBody] ApplyDiscountDto dto)
        {
            var result = await _discountService.ApplyDiscountAsync(dto.Code);
            if (!result.IsValid)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}