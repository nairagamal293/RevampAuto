namespace RevampAuto.DTOs
{
     public class OrderDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public ShippingDetailsDto ShippingDetails { get; set; }
        public string DiscountCode { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
    }

    public class CreateOrderDto
    {
        public string ShippingAddress { get; set; }
        public string DiscountCode { get; set; }
        public List<OrderItemCreateDto> Items { get; set; }
        public ShippingDetailsCreateDto ShippingDetails { get; set; }
    }

    public class OrderItemCreateDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
