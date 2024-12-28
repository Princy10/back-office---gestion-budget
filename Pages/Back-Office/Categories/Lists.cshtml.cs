using gestion_budget.DAL;
using gestion_budget.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace gestion_budget.Pages.Back_Office.Categories
{
    public class CategoriesModel : PageModel
    {
        private readonly AppDbContext _context;

        public CategoriesModel(AppDbContext context)
        {
            _context = context;
        }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;

        [BindProperty]
        public List<Category> DisplayedCategories { get; set; } = new List<Category>();

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            int pageSize = 2;

            var query = _context.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.ParentCategoryId == null);

            int totalCategories = await query.CountAsync();

            var parentCategories = await query
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            DisplayedCategories = parentCategories
                .SelectMany(parent => new[] { parent }
                    .Concat(parent.SubCategories ?? Enumerable.Empty<Category>()))
                .ToList();

            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling((double)totalCategories / pageSize);

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var categoryToDelete = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (categoryToDelete == null)
            {
                return NotFound("La catégorie demandée n'existe pas.");
            }

            try
            {
                _context.Categories.Remove(categoryToDelete);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "La catégorie a été supprimée avec succès.";
                return RedirectToPage("/Back-Office/Categories/lists", new { pageNumber = CurrentPage });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erreur lors de la suppression de la catégorie : " + ex.Message);
                return Page();
            }
        }
    }
}
