namespace ECommerceAPIProject.Models
{
    public class InvoiceRequest
    {
        public List<InvoiceItemRequest> Items { get; set; }
    }

    public class InvoiceItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}