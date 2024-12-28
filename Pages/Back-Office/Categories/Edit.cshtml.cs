using gestion_budget.DAL;
using gestion_budget.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace gestion_budget.Pages.Back_Office.Categories
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Category Category { get; set; }

        public List<Category> ParentCategories { get; set; } = new List<Category>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (Category == null)
            {
                return NotFound();
            }

            ParentCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var categoryToUpdate = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == Category.CategoryId);

            if (categoryToUpdate == null)
            {
                return NotFound();
            }

            categoryToUpdate.Name = Category.Name;
            categoryToUpdate.IsIncome = Category.IsIncome;
            categoryToUpdate.ParentCategoryId = Category.ParentCategoryId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("Une erreur s'est produite lors de la mise à jour de la catégorie.");
            }

            return RedirectToPage("/Back-Office/Categories/lists");
        }
    }
}
