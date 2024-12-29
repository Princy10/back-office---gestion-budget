using gestion_budget.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestion_budget.Pages.Back_Office.Account
{
    public class LogoutModel : PageModel
    {
        private readonly UserService _userService;

        public LogoutModel(UserService userService)
        {
            _userService = userService;
        }

        public IActionResult OnGet()
        {
            _userService.Logout();
            return RedirectToPage("/Back-Office/Account/login");
        }
    }
}
