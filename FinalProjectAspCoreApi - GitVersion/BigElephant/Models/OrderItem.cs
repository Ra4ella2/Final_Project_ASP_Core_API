using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BigElephant.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        // Количество товара
        public int Quantity { get; set; }

        // Цена товара на момент покупки (фиксируется)
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        // Навигация
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
