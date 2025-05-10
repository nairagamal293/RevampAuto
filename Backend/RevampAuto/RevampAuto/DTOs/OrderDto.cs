namespace RevampAuto.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string Status { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }

    public class CreateOrderDto
    {
        public string ShippingAddress { get; set; }
        public List<CreateOrderItemDto> OrderItems { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; }
    }
}
