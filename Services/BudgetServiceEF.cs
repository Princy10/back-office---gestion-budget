using gestion_budget.DAL;
using gestion_budget.Models;
using Microsoft.EntityFrameworkCore;

namespace gestion_budget.Services
{
    public class BudgetServiceEF
    {
        private readonly AppDbContext _context;

        public BudgetServiceEF(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Budget>> GetAllBudgetsAsync()
        {
            return await _context.Budgets
                .Include(b => b.Category)
                .Include(b => b.User)
                .ToListAsync();
        }

        public async Task<Budget?> GetBudgetByIdAsync(int id)
        {
            return await _context.Budgets
                .Include(b => b.Category)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BudgetId == id);
        }

        public async Task<List<Budget>> GetBudgetsByUserIdAsync(int userId)
        {
            return await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task AddBudgetAsync(Budget budget)
        {
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBudgetAsync(Budget budget)
        {
            var budgetToUpdate = await _context.Budgets
                .FirstOrDefaultAsync(b => b.BudgetId == budget.BudgetId);

            if (budgetToUpdate != null)
            {
                budgetToUpdate.BudgetAmount = budget.BudgetAmount;
                budgetToUpdate.StartDate = budget.StartDate;
                budgetToUpdate.EndDate = budget.EndDate;
                budgetToUpdate.CategoryId = budget.CategoryId;
                budgetToUpdate.UserId = budget.UserId;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteBudgetAsync(int id)
        {
            var budget = await GetBudgetByIdAsync(id);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<Budget> Budgets, int TotalPages)> GetPagedBudgetsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Budgets
                .Include(b => b.Category)
                .Include(b => b.User);

            int totalBudgets = await query.CountAsync();
            var budgets = await query
                .OrderByDescending(b => b.StartDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (budgets, (int)Math.Ceiling((double)totalBudgets / pageSize));
        }

        public async Task<List<Budget>> GetActiveBudgetsAsync()
        {
            var currentDate = DateTime.Now.Date;
            return await _context.Budgets
                .Include(b => b.Category)
                .Include(b => b.User)
                .Where(b => b.StartDate <= currentDate && b.EndDate >= currentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalBudgetForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Budgets
                .Where(b => b.StartDate >= startDate && b.EndDate <= endDate)
                .SumAsync(b => b.BudgetAmount);
        }
    }
}
