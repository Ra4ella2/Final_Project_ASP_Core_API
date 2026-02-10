using System.ComponentModel.DataAnnotations;

namespace BigElephant.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        // Владелец заказа (AspNetUsers.Id)
        [Required]
        public string UserId { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; }

        // Навигация
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
