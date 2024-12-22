namespace gestion_budget.Models.Views
{
    public class TransactionSummary
    {
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
