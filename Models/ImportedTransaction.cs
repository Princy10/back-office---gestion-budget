namespace gestion_budget.Models
{
    public class ImportedTransaction
    {
        public int ImportedTransactionId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Note { get; set; }
        public DateTime ImportedAt { get; set; }
        public User User { get; set; }
        public Category Category { get; set; }
    }
}
