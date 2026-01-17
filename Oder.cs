using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderManagement.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Order number is required")]
        [StringLength(50, ErrorMessage = "Order number cannot exceed 50 characters")]
        [Index(IsUnique = true)]
        [RegularExpression(@"^ORD-\d{8}-\d{4}$", ErrorMessage = "Order number format must be ORD-YYYYMMDD-XXXX")]
        public string OrderNumber { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 100 characters")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Customer email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Index(IsUnique = true)]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Order date is required")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime OrderDate { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime? DeliveryDate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public virtual Product Product { get; set; }

        // Computed property for status
        [NotMapped]
        public string Status
        {
            get
            {
                return DeliveryDate.HasValue ? "Delivered" : "Pending";
            }
        }
    }
}