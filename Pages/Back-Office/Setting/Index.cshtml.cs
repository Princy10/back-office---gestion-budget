using gestion_budget.Models;
using gestion_budget.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestion_budget.Pages.Back_Office.Setting
{
    public class IndexModel : PageModel
    {
        private readonly DefaultSettingService _service;

        public IndexModel(DefaultSettingService service)
        {
            _service = service;
        }

        public List<DefaultSetting> DefaultSettings { get; set; }

        public async Task OnGetAsync()
        {
            DefaultSettings = await _service.GetAllDefaultSettingsAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _service.DeleteDefaultSettingAsync(id);
                TempData["SuccessMessage"] = "Le paramètre a été supprimé avec succès.";
                return RedirectToPage("/Back-Office/Setting/index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erreur lors de la suppression : {ex.Message}");
                return RedirectToPage("/Back-Office/Setting/index");
            }
        }

    }
}
