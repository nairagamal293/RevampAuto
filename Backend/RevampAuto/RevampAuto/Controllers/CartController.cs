using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RevampAuto.Data;
using RevampAuto.DTOs;
using RevampAuto.Models;
using System.Security.Claims;

namespace RevampAuto.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, UserManager<User> userManager, ILogger<CartController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: api/cart
        [HttpGet]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    return Ok(new CartDto
                    {
                        UserId = userId,
                        Items = new List<CartItemDto>(),
                        TotalPrice = 0
                    });
                }

                var cartDto = new CartDto
                {
                    Id = cart.Id,
                    UserId = cart.UserId,
                    Items = cart.Items.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,
                        ProductPrice = ci.Product.Price,
                        ProductImage = ci.Product.Images.FirstOrDefault(i => i.IsMainImage)?.ImagePath ?? "",
                        Quantity = ci.Quantity,
                        ItemTotal = ci.Quantity * ci.Product.Price
                    }).ToList(),
                    TotalPrice = cart.Items.Sum(ci => ci.Quantity * ci.Product.Price)
                };

                return Ok(cartDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return StatusCode(500, "An error occurred while retrieving the cart");
            }
        }

        // POST: api/cart/add
        [HttpPost("add")]
        public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Validate the product exists and has sufficient stock
                var product = await _context.Products.FindAsync(addToCartDto.ProductId);
                if (product == null)
                {
                    return NotFound("Product not found");
                }

                if (product.StockQuantity < addToCartDto.Quantity)
                {
                    return BadRequest("Insufficient stock available");
                }

                // Get or create cart for user
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        Items = new List<CartItem>()
                    };
                    _context.Carts.Add(cart);
                }

                // Check if item already exists in cart
                var existingItem = cart.Items.FirstOrDefault(ci => ci.ProductId == addToCartDto.ProductId);
                if (existingItem != null)
                {
                    // Update quantity if item exists
                    existingItem.Quantity += addToCartDto.Quantity;
                }
                else
                {
                    // Add new item to cart
                    cart.Items.Add(new CartItem
                    {
                        ProductId = addToCartDto.ProductId,
                        Quantity = addToCartDto.Quantity
                    });
                }

                await _context.SaveChangesAsync();

                return await GetCart();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, "An error occurred while adding item to cart");
            }
        }

        // PUT: api/cart/items/{itemId}
        [HttpPut("items/{itemId}")]
        public async Task<ActionResult<CartDto>> UpdateCartItem(int itemId, [FromBody] UpdateCartItemDto updateCartItemDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Find the cart item
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.Cart.UserId == userId);

                if (cartItem == null)
                {
                    return NotFound("Cart item not found");
                }

                // Validate the product has sufficient stock
                if (cartItem.Product.StockQuantity < updateCartItemDto.Quantity)
                {
                    return BadRequest("Insufficient stock available");
                }

                // Update quantity
                cartItem.Quantity = updateCartItemDto.Quantity;
                cartItem.Cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetCart();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return StatusCode(500, "An error occurred while updating cart item");
            }
        }

        // DELETE: api/cart/items/{itemId}
        [HttpDelete("items/{itemId}")]
        public async Task<ActionResult<CartDto>> RemoveCartItem(int itemId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Find the cart item
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.Cart.UserId == userId);

                if (cartItem == null)
                {
                    return NotFound("Cart item not found");
                }

                _context.CartItems.Remove(cartItem);
                cartItem.Cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetCart();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item");
                return StatusCode(500, "An error occurred while removing cart item");
            }
        }

        // DELETE: api/cart/clear
        [HttpDelete("clear")]
        public async Task<ActionResult> ClearCart()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    return NotFound("Cart not found");
                }

                _context.CartItems.RemoveRange(cart.Items);
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, "An error occurred while clearing the cart");
            }
        }

        // GET: api/cart/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCartItemCount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var count = await _context.Carts
                    .Where(c => c.UserId == userId)
                    .SelectMany(c => c.Items)
                    .SumAsync(ci => ci.Quantity);

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count");
                return StatusCode(500, "An error occurred while getting cart item count");
            }
        }

        // Add this method to your CartController
        [HttpPost("merge")]
        [Authorize]
        public async Task<ActionResult<CartDto>> MergeGuestCart([FromBody] GuestCartDto guestCartDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Get the user's cart
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId) ?? new Cart
                    {
                        UserId = userId,
                        Items = new List<CartItem>()
                    };

                if (cart.Id == 0)
                {
                    _context.Carts.Add(cart);
                }

                // Process guest cart items
                foreach (var guestItem in guestCartDto.Items)
                {
                    // Validate product exists
                    var product = await _context.Products.FindAsync(guestItem.ProductId);
                    if (product == null) continue;

                    // Check if item already exists in cart
                    var existingItem = cart.Items.FirstOrDefault(ci => ci.ProductId == guestItem.ProductId);

                    if (existingItem != null)
                    {
                        // Update quantity if item exists
                        existingItem.Quantity += guestItem.Quantity;
                    }
                    else
                    {
                        // Add new item to cart
                        cart.Items.Add(new CartItem
                        {
                            ProductId = guestItem.ProductId,
                            Quantity = guestItem.Quantity
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return await GetCart();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging guest cart");
                return StatusCode(500, "An error occurred while merging guest cart");
            }
        }

        
        
    }
}