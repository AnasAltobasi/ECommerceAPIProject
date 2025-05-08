using ECommerceAPIProject.EntityFrameworkCore.Entities;
using ECommerceAPIProject.Models;

public class Invoice
{
    public Invoice()
    {
        Details = new List<InvoiceDetail>();
    }

    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public decimal TotalAmount { get; set; }
    public ICollection<InvoiceDetail> Details { get; set; }
}