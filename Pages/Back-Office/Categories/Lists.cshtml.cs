using gestion_budget.DAL;
using gestion_budget.Models;
using gestion_budget.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace gestion_budget.Pages.Back_Office.Categories
{
    public class ListsModel : PageModel
    {
        private readonly CategoryService _categoryService;

        public ListsModel(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        [BindProperty]
        public List<Category> DisplayedCategories { get; set; } = new List<Category>();

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            int pageSize = 2;
            (var parentCategories, int totalPages) = await _categoryService.GetPagedCategoriesAsync(pageNumber, pageSize);
            DisplayedCategories = parentCategories
                .SelectMany(parent => new[] { parent }.Concat(parent.SubCategories ?? Enumerable.Empty<Category>()))
                .ToList();
            CurrentPage = pageNumber;
            TotalPages = totalPages;
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                TempData["SuccessMessage"] = "La catégorie a été supprimée avec succès.";
                return RedirectToPage(new { pageNumber = CurrentPage });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erreur lors de la suppression : {ex.Message}");
                return await OnGetAsync(CurrentPage);
            }
        }
    }
}
