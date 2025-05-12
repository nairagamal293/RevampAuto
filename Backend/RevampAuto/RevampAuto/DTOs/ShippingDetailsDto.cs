namespace RevampAuto.DTOs
{
    public class ShippingDetailsDto
    {
        public string FullName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class ShippingDetailsCreateDto : ShippingDetailsDto { }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; }
        public string TrackingNumber { get; set; }
    }
}
