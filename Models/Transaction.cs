using System.ComponentModel.DataAnnotations;

namespace gestion_budget.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; }
        public string Note { get; set; }
        public User User { get; set; }
        public Category Category { get; set; }
    }
}
