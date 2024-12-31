using gestion_budget.DAL;
using gestion_budget.Models;
using Microsoft.EntityFrameworkCore;

namespace gestion_budget.Services
{
    public class DefaultSettingService
    {
        private readonly AppDbContext _context;

        public DefaultSettingService(AppDbContext context)
        {
            _context = context;
        }

        // Récupérer tous les paramètres par défaut
        public async Task<List<DefaultSetting>> GetAllDefaultSettingsAsync()
        {
            return await _context.DefaultSettings
                .Include(ds => ds.User)
                .Include(ds => ds.Category)
                .ToListAsync();
        }

        // Récupérer les paramètres par défaut d'un utilisateur spécifique
        public async Task<List<DefaultSetting>> GetDefaultSettingsByUserAsync(int userId)
        {
            return await _context.DefaultSettings
                .Where(ds => ds.UserId == userId)
                .Include(ds => ds.User)
                .Include(ds => ds.Category)
                .ToListAsync();
        }

        // Ajouter un nouveau paramètre par défaut
        public async Task AddDefaultSettingAsync(DefaultSetting defaultSetting)
        {
            try
            {
                _context.DefaultSettings.Add(defaultSetting);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajout du DefaultSetting : {ex.Message}");
                throw;
            }
        }


        // Récupérer un paramètre par son ID
        public async Task<DefaultSetting> GetDefaultSettingByIdAsync(int id)
        {
            return await _context.DefaultSettings
                .Include(ds => ds.User)
                .Include(ds => ds.Category)
                .FirstOrDefaultAsync(ds => ds.DefaultSettingId == id);
        }

        // Mettre à jour un paramètre par défaut
        public async Task UpdateDefaultSettingAsync(DefaultSetting defaultSetting)
        {
            Console.WriteLine($"DefaultSettingId reçu : {defaultSetting.DefaultSettingId}");

            var existing = await _context.DefaultSettings
                .FirstOrDefaultAsync(ds => ds.DefaultSettingId == defaultSetting.DefaultSettingId);

            if (existing == null)
            {
                Console.WriteLine("Aucun DefaultSetting trouvé avec cet ID.");
                return;
            }

            Console.WriteLine($"DefaultSetting trouvé : {existing.DefaultSettingId}, UserId : {existing.UserId}, CategoryId : {existing.CategoryId}");

            existing.UserId = defaultSetting.UserId;
            existing.CategoryId = defaultSetting.CategoryId;
            existing.DefaultLimit = defaultSetting.DefaultLimit;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }


        // Supprimer un paramètre par défaut
        public async Task DeleteDefaultSettingAsync(int id)
        {
            var setting = await _context.DefaultSettings.FindAsync(id);
            if (setting != null)
            {
                _context.DefaultSettings.Remove(setting);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<DefaultSetting> DefaultSettings, int TotalPages)> GetPagedDefaultSettingsAsync(int pageNumber, int pageSize)
        {
            var query = _context.DefaultSettings
                .Include(ds => ds.User)
                .Include(ds => ds.Category);

            int totalSettings = await query.CountAsync();
            var defaultSettings = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int totalPages = (int)Math.Ceiling((double)totalSettings / pageSize);
            return (defaultSettings, totalPages);
        }
    }
}
