namespace RevampAuto.DTOs
{
    public class DiscountDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Percentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxUses { get; set; }
        public int CurrentUses { get; set; }
        public bool IsActive { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
    }

    public class CreateDiscountDto
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Percentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxUses { get; set; }
        public int? CategoryId { get; set; }
        public int? ProductId { get; set; }
    }

    public class UpdateDiscountDto
    {
        public string Description { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxUses { get; set; }
        public bool IsActive { get; set; }
    }

    public class ApplyDiscountDto
    {
        public string Code { get; set; }
    }
}
