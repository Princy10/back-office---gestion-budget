using gestion_budget.DAL;
using gestion_budget.Models;
using Microsoft.EntityFrameworkCore;

namespace gestion_budget.Services
{
    public class CategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetParentCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            var categoryToUpdate = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);

            if (categoryToUpdate != null)
            {
                categoryToUpdate.Name = category.Name;
                categoryToUpdate.IsIncome = category.IsIncome;
                categoryToUpdate.ParentCategoryId = category.ParentCategoryId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await GetCategoryByIdAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<Category> Categories, int TotalPages)> GetPagedCategoriesAsync(int pageNumber, int pageSize)
        {
            var query = _context.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.ParentCategoryId == null);

            int totalCategories = await query.CountAsync();
            var parentCategories = await query
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (parentCategories, (int)Math.Ceiling((double)totalCategories / pageSize));
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .ToListAsync();
        }

        public async Task<List<Category>> GetIncomeCategorie()
        {
            return await _context.Categories
                .Where(c => c.IsIncome == false)
                .ToListAsync();
        }
    }
}
