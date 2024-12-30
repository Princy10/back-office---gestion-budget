using gestion_budget.Models;
using gestion_budget.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gestion_budget.Pages.Back_Office.Setting
{
    public class CreateModel : PageModel
    {
        private readonly DefaultSettingService _defaultSettingService;
        private readonly UserService _userService;
        //private readonly CategoryService _categoryService;

        public CreateModel(DefaultSettingService defaultSettingService, UserService userService, /*CategoryService categoryService*/)
        {
            _defaultSettingService = defaultSettingService;
            _userService = userService;
            //_categoryService = categoryService;
        }

        [BindProperty]
        public DefaultSetting DefaultSetting { get; set; }

        public SelectList Users { get; set; }
        public SelectList Categories { get; set; }

        public async Task OnGetAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            //var categories = await _categoryService.GetAllCategoriesAsync();

            Users = new SelectList(users, "UserId", "UserName");
            //Categories = new SelectList(categories, "CategoryId", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var users = await _userService.GetAllUsersAsync();
                //var categories = await _categoryService.GetAllCategoriesAsync();

                Users = new SelectList(users, "UserId", "UserName");
                //Categories = new SelectList(categories, "CategoryId", "Name");

                return Page();
            }

            await _defaultSettingService.AddDefaultSettingAsync(DefaultSetting);
            return RedirectToPage("./Index");
        }
    }
}
