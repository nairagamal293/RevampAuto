using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RevampAuto.Models
{
    public class Discount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Percentage { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxUses { get; set; }
        public int CurrentUses { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Optional: Category or Product specific discount
        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        public int? ProductId { get; set; }
        public Product Product { get; set; }
    }
}
