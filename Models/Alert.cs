namespace gestion_budget.Models
{
    public class Alert
    {
        public int AlertId { get; set; }
        public int UserId { get; set; }
        public int BudgetId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public User User { get; set; }
        public Budget Budget { get; set; }
    }
}
