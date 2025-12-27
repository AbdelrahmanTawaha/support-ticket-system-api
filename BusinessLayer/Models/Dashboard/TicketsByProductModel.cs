namespace BusinessLayer.Models.Dashboard
{
    public class TicketsByProductModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Count { get; set; }
    }
}
