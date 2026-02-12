namespace BigElephant.Models
{
    public class AdminLog
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public string AdminId { get; set; } = null!;

        public string Action { get; set; } = null!;
    }
}
