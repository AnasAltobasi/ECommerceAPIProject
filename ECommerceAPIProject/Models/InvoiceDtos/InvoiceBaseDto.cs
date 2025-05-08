namespace ECommerceAPIProject.Models.InvoiceDtos
{
    public class InvoiceBaseDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class InvoiceDto : InvoiceBaseDto
    {
        public List<InvoiceItemDto> Items { get; set; }
    }

    public class InvoiceSimpleDto : InvoiceBaseDto
    {
        public List<InvoiceItemDto> Details { get; set; }
    }
}
