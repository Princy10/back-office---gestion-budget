using gestion_budget.Models;
using gestion_budget.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestion_budget.Pages.Back_Office.Categories
{
    public class EditModel : PageModel
    {
        private readonly CategoryService _categoryService;

        public EditModel(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public Category Category { get; set; }
        public List<Category> ParentCategories { get; set; } = new List<Category>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Category = await _categoryService.GetCategoryByIdAsync(id);
            if (Category == null)
            {
                return NotFound();
            }
            ParentCategories = await _categoryService.GetParentCategoriesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ParentCategories = await _categoryService.GetParentCategoriesAsync();
                return Page();
            }

            try
            {
                await _categoryService.UpdateCategoryAsync(Category);
                return RedirectToPage("/Back-Office/Categories/lists");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erreur : {ex.Message}");
                ParentCategories = await _categoryService.GetParentCategoriesAsync();
                return Page();
            }
        }
    }
}
