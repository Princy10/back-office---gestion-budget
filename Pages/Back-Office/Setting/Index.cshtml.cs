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

        public List<DefaultSetting> DisplayedDefaultSettings { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;

        public async Task OnGetAsync(int pageNumber = 1)
        {
            int pageSize = 10;
            var result = await _service.GetPagedDefaultSettingsAsync(pageNumber, pageSize);
            DisplayedDefaultSettings = result.DefaultSettings;
            TotalPages = result.TotalPages;
            CurrentPage = pageNumber;
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
