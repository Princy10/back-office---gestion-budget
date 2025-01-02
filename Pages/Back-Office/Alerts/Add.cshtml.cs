using gestion_budget.Models;
using gestion_budget.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestion_budget.Pages.Back_Office.Alerts
{
    public class AddModel : PageModel
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AlertService _alertService;
        private readonly BudgetServiceEF _budgetService;

        public AddModel(IHttpContextAccessor httpContextAccessor, AlertService alertService, BudgetServiceEF budgetService)
        {
            _httpContextAccessor = httpContextAccessor;
            _alertService = alertService;
            _budgetService = budgetService;
        }

        [BindProperty]
        public Alert Alert { get; set; }
        public List<Budget> Budgets { get; set; } = new List<Budget>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToPage("/Back-Office/Account/Login");
            }

            Alert = new Alert { UserId = userId.Value };

            Budgets = await _budgetService.GetAllBudgetsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    Budgets = await _budgetService.GetAllBudgetsAsync();
            //    return Page();
            //}

            try
            {
                await _alertService.AddAlertAsync(Alert);
                return RedirectToPage("/Back-Office/Alerts/lists");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erreur : {ex.Message}");
                Budgets = await _budgetService.GetAllBudgetsAsync();
                return Page();
            }
        }
    }
}
