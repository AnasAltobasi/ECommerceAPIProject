using ECommerceAPIProject.EntityFrameworkCore;
using ECommerceAPIProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceAPIProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public InvoicesController(AppDbContext context) => _context = context;

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllInvoices()
        {
            var invoices = await _context.Invoices
                .Include(i => i.User)
                .Include(i => i.Details)
                .ThenInclude(d => d.Product)
                .ToListAsync();

            return Ok(invoices);
        }

        [Authorize(Roles = "Visitor")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyInvoices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var invoices = await _context.Invoices
                .Where(i => i.UserId == userId)
                .Include(i => i.Details)
                .ThenInclude(d => d.Product)
                .ToListAsync();

            return Ok(invoices);
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.User)
                .Include(i => i.Details)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            // Authorization check
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && invoice.UserId != userId)
                return Forbid();

            return Ok(invoice);
        }

        [Authorize(Roles = "Visitor")]
        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] InvoiceRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var invoice = new Invoice { UserId = userId };

                foreach (var item in request.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);

                    if (product == null)
                        return BadRequest($"Product with ID {item.ProductId} not found");

                    invoice.Details.Add(new InvoiceDetail
                    {
                        ProductId = product.Id,
                        Price = product.Price,
                        Quantity = item.Quantity
                    });
                }

                invoice.TotalAmount = invoice.Details.Sum(d => d.Price * d.Quantity);
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] InvoiceUpdateRequest request)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            // Update invoice details
            invoice.Details.Clear();
            foreach (var item in request.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    return BadRequest($"Product with ID {item.ProductId} not found");

                invoice.Details.Add(new InvoiceDetail
                {
                    ProductId = product.Id,
                    Price = product.Price,
                    Quantity = item.Quantity
                });
            }

            invoice.TotalAmount = invoice.Details.Sum(d => d.Price * d.Quantity);
            await _context.SaveChangesAsync();
            return Ok(invoice);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound();

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}