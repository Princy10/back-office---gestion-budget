using gestion_budget.DAL;
using gestion_budget.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace gestion_budget.Pages.Back_Office.Categories
{
    public class AddModel : PageModel
    {
        private readonly AppDbContext _context;

        public AddModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Category Category { get; set; }

        public List<Category> ParentCategories { get; set; } = new List<Category>();

        public async Task<IActionResult> OnGetAsync()
        {
            ParentCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ParentCategories = await _context.Categories
                    .Where(c => c.ParentCategoryId == null)
                    .ToListAsync();
                return Page();
            }

            try
            {
                if (Category.ParentCategoryId.HasValue)
                {
                    var parentCategory = await _context.Categories.FindAsync(Category.ParentCategoryId);
                    if (parentCategory == null)
                    {
                        ModelState.AddModelError("Category.ParentCategoryId", "La catégorie parente spécifiée n'existe pas.");
                        ParentCategories = await _context.Categories
                            .Where(c => c.ParentCategoryId == null)
                            .ToListAsync();
                        return Page();
                    }
                }

                var newCategory = new Category
                {
                    Name = Category.Name,
                    IsIncome = Category.IsIncome,
                    ParentCategoryId = Category.ParentCategoryId,
                    SubCategories = new List<Category>(),
                    Transactions = new List<Transaction>(),
                    Budgets = new List<Budget>()
                };

                _context.Categories.Add(newCategory);
                await _context.SaveChangesAsync();

                return RedirectToPage("/Back-Office/Categories/lists");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erreur : {ex.Message}");
                ParentCategories = await _context.Categories
                    .Where(c => c.ParentCategoryId == null)
                    .ToListAsync();
                return Page();
            }
        }
    }
}
