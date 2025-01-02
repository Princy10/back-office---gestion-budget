using gestion_budget.Models;
using gestion_budget.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestion_budget.Pages.Back_Office.Alerts
{
    public class EditModel : PageModel
    {
        private readonly AlertService _alertService;
        private readonly BudgetServiceEF _budgetService;

        public EditModel(AlertService alertService, BudgetServiceEF budgetService)
        {
            _alertService = alertService;
            _budgetService = budgetService;
        }

        [BindProperty]
        public Alert Alert { get; set; }
        public List<Budget> Budgets { get; set; } = new List<Budget>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Alert = await _alertService.GetAlertByIdAsync(id);
            if (Alert == null)
            {
                return NotFound();
            }
            Budgets = await _budgetService.GetAllBudgetsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Budgets = await _budgetService.GetAllBudgetsAsync();
                return Page();
            }

            try
            {
                await _alertService.UpdateAlertAsync(Alert);
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
