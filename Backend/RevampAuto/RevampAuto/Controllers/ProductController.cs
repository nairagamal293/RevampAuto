using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RevampAuto.Data;
using RevampAuto.DTOs;
using RevampAuto.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RevampAuto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductsController> _logger;


        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<ProductsController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }


        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImageUrls = p.Images.Select(i => i.ImagePath).ToList(),
                    MainImageUrl = p.Images.Where(i => i.IsMainImage).Select(i => i.ImagePath).FirstOrDefault()
                })
                .ToListAsync();
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImageUrls = p.Images.Select(i => i.ImagePath).ToList(),
                    MainImageUrl = p.Images.Where(i => i.IsMainImage).Select(i => i.ImagePath).FirstOrDefault()
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // POST: api/products
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ProductDto>> PostProduct([FromForm] CreateProductDto createProductDto)
        {
            var product = new Product
            {
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                Price = createProductDto.Price,
                StockQuantity = createProductDto.StockQuantity,
                CategoryId = createProductDto.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPost("{id}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImages(int id, [FromForm] List<IFormFile> files, [FromQuery] bool setMain = false)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return BadRequest("No files uploaded");

                // Ensure WebRootPath is available
                if (string.IsNullOrEmpty(_environment.WebRootPath))
                {
                    _environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uploadedFiles = new List<ProductImage>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var productImage = new ProductImage
                        {
                            ImagePath = $"/uploads/products/{uniqueFileName}",
                            ProductId = id,
                            IsMainImage = setMain && files.IndexOf(file) == 0
                        };

                        uploadedFiles.Add(productImage);
                    }
                }

                if (setMain)
                {
                    var existingMainImages = await _context.ProductImages
                        .Where(pi => pi.ProductId == id && pi.IsMainImage)
                        .ToListAsync();

                    foreach (var img in existingMainImages)
                    {
                        img.IsMainImage = false;
                    }
                }

                await _context.ProductImages.AddRangeAsync(uploadedFiles);
                await _context.SaveChangesAsync();

                return Ok(uploadedFiles.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImagePath = i.ImagePath,
                    IsMainImage = i.IsMainImage
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading images");
                return StatusCode(500, "Internal server error while uploading images");
            }
        }

        // PUT: api/products/5/images/3/setmain
        [Authorize(Roles = "Admin")]
        [HttpPut("{productId}/images/{imageId}/setmain")]
        public async Task<ActionResult> SetMainImage(int productId, int imageId)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(pi => pi.Id == imageId && pi.ProductId == productId);

            if (image == null)
            {
                return NotFound("Image not found");
            }

            // Reset all main images for this product
            var existingMainImages = await _context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.IsMainImage)
                .ToListAsync();

            foreach (var img in existingMainImages)
            {
                img.IsMainImage = false;
            }

            // Set the new main image
            image.IsMainImage = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/products/5/images/3
        [Authorize(Roles = "Admin")]
        [HttpDelete("{productId}/images/{imageId}")]
        public async Task<ActionResult> DeleteImage(int productId, int imageId)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(pi => pi.Id == imageId && pi.ProductId == productId);

            if (image == null)
            {
                return NotFound("Image not found");
            }

            // Delete the physical file
            var filePath = Path.Combine(_environment.WebRootPath, image.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/products/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, UpdateProductDto updateProductDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = updateProductDto.Name;
            product.Description = updateProductDto.Description;
            product.Price = updateProductDto.Price;
            product.StockQuantity = updateProductDto.StockQuantity;
            product.CategoryId = updateProductDto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        // GET: api/products/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
        {
            // First check if category exists
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                return NotFound("Category not found");
            }

            var products = await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImageUrls = p.Images.Select(i => i.ImagePath).ToList(),
                    MainImageUrl = p.Images.Where(i => i.IsMainImage).Select(i => i.ImagePath).FirstOrDefault()
                })
                .ToListAsync();

            return products;
        }

        // DELETE: api/products/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Delete all associated images
            foreach (var image in product.Images)
            {
                var filePath = Path.Combine(_environment.WebRootPath, image.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}