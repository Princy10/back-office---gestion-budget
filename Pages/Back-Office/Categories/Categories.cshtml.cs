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
            int pageSize = 1;

            // R�cup�rer les cat�gories parentales avec le comptage total
            var query = _context.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.ParentCategoryId == null);

            int totalCategories = await query.CountAsync();

            // Appliquer la pagination directement dans la requ�te SQL
            var parentCategories = await query
                .OrderBy(c => c.Name) // Trier les cat�gories parentales par nom
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pr�parer les cat�gories parentales et leurs sous-cat�gories pour l'affichage
            DisplayedCategories = parentCategories
                .SelectMany(parent => new[] { parent }
                    .Concat(parent.SubCategories ?? Enumerable.Empty<Category>()))
                .ToList();

            // Calculer le nombre total de pages
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling((double)totalCategories / pageSize);

            return Page();
        }
    }
}
