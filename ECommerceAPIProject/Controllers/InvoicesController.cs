using ECommerceAPIProject.EntityFrameworkCore;
using ECommerceAPIProject.EntityFrameworkCore.Entities;
using ECommerceAPIProject.Models;
using ECommerceAPIProject.Models.InvoiceDtos;
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
                .Select(i => new InvoiceSimpleDto
                {
                    Id = i.Id,
                    Date = i.Date,
                    UserId = i.UserId,
                    UserName = i.User != null ? i.User.UserName : "Deleted User",
                    TotalAmount = i.TotalAmount,
                    Details = i.Details.Select(d => new InvoiceItemDto
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product != null ? d.Product.EnglishName : "Deleted Product",
                        Price = d.Price,
                        Quantity = d.Quantity
                    }).ToList()
                })
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
                .Select(i => new InvoiceSimpleDto
                {
                    Id = i.Id,
                    Date = i.Date,
                    TotalAmount = i.TotalAmount,
                    Details = i.Details.Select(d => new InvoiceItemDto
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product.EnglishName,
                        Price = d.Price,
                        Quantity = d.Quantity
                    }).ToList()
                })
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
                .Select(i => new InvoiceDto
                {
                    Id = i.Id,
                    Date = i.Date,
                    UserId = i.UserId,
                    UserName = i.User != null ? i.User.UserName : "Deleted User",
                    TotalAmount = i.TotalAmount,
                    Items = i.Details.Select(d => new InvoiceItemDto
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product != null ? d.Product.EnglishName : "Deleted Product",
                        Price = d.Price,
                        Quantity = d.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            // Authorization check
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && invoice.UserId != currentUserId)
                return Forbid();

            return Ok(invoice);
        }

        [Authorize(Roles = "Visitor")]
        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] InvoiceRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var invoice = new Invoice { UserId = userId };

                foreach (var item in request.Items)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId && !p.IsDeleted);

                    if (product == null)
                        return BadRequest($"Product with ID {item.ProductId} not found or deleted");

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

                return Ok(new InvoiceDto
                {
                    Id = invoice.Id,
                    Date = invoice.Date,
                    TotalAmount = invoice.TotalAmount,
                    Items = invoice.Details.Select(d => new InvoiceItemDto
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product.EnglishName,
                        Price = d.Price,
                        Quantity = d.Quantity
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] InvoiceRequestDto request)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            invoice.Details.Clear();
            foreach (var item in request.Items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId && !p.IsDeleted);

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

            return Ok(new InvoiceSimpleDto
            {
                Id = invoice.Id,
                Date = invoice.Date,
                TotalAmount = invoice.TotalAmount,
                Details = invoice.Details.Select(d => new InvoiceItemDto
                {
                    ProductId = d.ProductId,
                    ProductName = d.Product.EnglishName,
                    Price = d.Price,
                    Quantity = d.Quantity
                }).ToList()
            });
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