using gestion_budget.DAL;
using gestion_budget.Models;
using Microsoft.EntityFrameworkCore;

namespace gestion_budget.Services
{
    public class BudgetAlertService
    {
        private readonly AppDbContext _context;

        public BudgetAlertService(AppDbContext context)
        {
            _context = context;
        }
        public async Task CheckBudgetExceedancesAsync(int userId)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            foreach (var budget in budgets)
            {
                var totalSpent = await _context.Transactions
                    .Where(t => t.UserId == userId && t.CategoryId == budget.CategoryId &&
                                t.TransactionDate >= budget.StartDate && t.TransactionDate <= budget.EndDate)
                    .SumAsync(t => t.Amount);

                if (totalSpent > budget.BudgetAmount)
                {
                    var exceedAmount = totalSpent - budget.BudgetAmount;

                    var existingAlert = await _context.Alerts
                        .AnyAsync(a => a.UserId == userId && a.BudgetId == budget.BudgetId &&
                                       a.Message.Contains("Dépassement"));

                    if (!existingAlert)
                    {
                        var alert = new Alert
                        {
                            UserId = userId,
                            BudgetId = budget.BudgetId,
                            Message = $"Dépassement de budget détecté pour la catégorie {budget.Category.Name} : dépassement de {exceedAmount:C}.",
                            CreatedAt = DateTime.Now
                        };

                        _context.Alerts.Add(alert);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
