using gestion_budget.DAL;
using gestion_budget.Models;
using Microsoft.EntityFrameworkCore;

namespace gestion_budget.Services
{
    public class AlertService
    {
        private readonly AppDbContext _context;

        public AlertService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Alert>> GetAlertsAsync()
        {
            return await _context.Alerts
                .Include(a => a.User)
                .Include(a => a.Budget)
                .ThenInclude(b => b.Category) // Inclure la catégorie
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Alert?> GetAlertByIdAsync(int id)
        {
            return await _context.Alerts
                .Include(a => a.User)
                .Include(a => a.Budget)
                .FirstOrDefaultAsync(a => a.AlertId == id);
        }

        public async Task AddAlertAsync(Alert alert)
        {
            alert.CreatedAt = DateTime.Now;
            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAlertAsync(Alert alert)
        {
            var alertToUpdate = await _context.Alerts
                .FirstOrDefaultAsync(a => a.AlertId == alert.AlertId);

            if (alertToUpdate != null)
            {
                alertToUpdate.Message = alert.Message;
                alertToUpdate.BudgetId = alert.BudgetId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAlertAsync(int id)
        {
            var alert = await GetAlertByIdAsync(id);
            if (alert != null)
            {
                _context.Alerts.Remove(alert);
                await _context.SaveChangesAsync();
            }
        }
    }

}
