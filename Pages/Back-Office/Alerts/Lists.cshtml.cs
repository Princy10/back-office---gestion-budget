using gestion_budget.Models;
using gestion_budget.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestion_budget.Pages.Back_Office.Alerts
{
    public class ListsModel : PageModel
    {
        private readonly AlertService _alertService;

        public ListsModel(AlertService alertService)
        {
            _alertService = alertService;
        }

        public List<Alert> Alerts { get; set; } = new List<Alert>();

        public async Task<IActionResult> OnGetAsync()
        {
            Alerts = await _alertService.GetAlertsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _alertService.DeleteAlertAsync(id);
                TempData["SuccessMessage"] = "L'alerte a été supprimée avec succès.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erreur lors de la suppression : {ex.Message}");
                return await OnGetAsync();
            }
        }
    }
}
