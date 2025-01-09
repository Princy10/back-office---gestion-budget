using System.ComponentModel.DataAnnotations;

namespace gestion_budget.Models.Views
{
    public class UserBalance
    {
        public int UserId { get; set; }
        public decimal Balance { get; set; }
    }
}
