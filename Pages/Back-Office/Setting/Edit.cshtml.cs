using gestion_budget.Models;
using gestion_budget.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gestion_budget.Pages.Back_Office.Setting
{
    public class EditModel : PageModel
    {
        private readonly DefaultSettingService _defaultSettingService;
        private readonly UserService _userService;
        private readonly CategoryService _categoryService;

        public EditModel(DefaultSettingService defaultSettingService, UserService userService, CategoryService categoryService)
        {
            _defaultSettingService = defaultSettingService;
            _userService = userService;
            _categoryService = categoryService;
        }

        [BindProperty]
        public DefaultSetting DefaultSetting { get; set; }
        public SelectList Users { get; set; }
        public SelectList Categories { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            DefaultSetting = await _defaultSettingService.GetDefaultSettingByIdAsync(id);
            if (DefaultSetting == null)
            {
                return NotFound();
            }

            var users = await _userService.GetUserAdminAsync();
            var categories = await _categoryService.GetIncomeCategorie();

            Users = new SelectList(users, "UserId", "UserName", DefaultSetting.UserId);
            Categories = new SelectList(categories, "CategoryId", "Name", DefaultSetting.CategoryId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    var users = await _userService.GetUserAdminAsync();
            //    var categories = await _categoryService.GetIncomeCategorie();

            //    Users = new SelectList(users, "UserId", "UserName");
            //    Categories = new SelectList(categories, "CategoryId", "Name");
            //    return Page();
            //}

            try
            {
                await _defaultSettingService.UpdateDefaultSettingAsync(DefaultSetting);
                return RedirectToPage("/Back-Office/Setting/index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erreur : {ex.Message}");
                return Page();
            }
        }
    }
}
