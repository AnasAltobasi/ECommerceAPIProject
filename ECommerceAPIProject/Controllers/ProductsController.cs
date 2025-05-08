using ECommerceAPIProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceAPIProject.EntityFrameworkCore;
using ECommerceAPIProject.EntityFrameworkCore.Entities;

namespace ECommerceAPIProject.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]

    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetProducts(int page = 1, int pageSize = 10)
        {
            var products = await _context.Products
                .Where(p => !p.IsDeleted)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(product.ArabicName) || string.IsNullOrEmpty(product.EnglishName))
            {
                ModelState.AddModelError("Validation", "Arabic and English names are required");
                return BadRequest(ModelState);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto updateDto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null || product.IsDeleted)
                return NotFound("Product not found");

            product.ArabicName = updateDto.ArabicName;
            product.EnglishName = updateDto.EnglishName;
            product.Price = updateDto.Price;

            await _context.SaveChangesAsync();
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            product.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}