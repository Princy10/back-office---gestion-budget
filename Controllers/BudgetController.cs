using gestion_budget.Models;
using System.Security.Claims;
using gestion_budget.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using System;

namespace gestion_budget.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly BudgetService _budgetService;

        public BudgetController()
        {
            var connectionString = "Server=DESKTOP-7A266J8;Database=BudgetManagement;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";
            _budgetService = new BudgetService(connectionString);
        }
        public IActionResult List()
        {
            var budgets = _budgetService.GetBudgets();
            return View(budgets);
        }

        public IActionResult Add()
        {
            var categories = _budgetService.GetCategories();
            ViewBag.Categories = categories ?? new List<Category>();
            ViewBag.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return View();
        }

        [HttpPost]
        public IActionResult Add(int CategoryId, int UserId, decimal BudgetAmount, DateTime StartDate, DateTime EndDate)
        {
            try
            {
                var budget = new Budget
                {
                    CategoryId = CategoryId,
                    UserId = UserId,
                    BudgetAmount = BudgetAmount,
                    StartDate = StartDate,
                    EndDate = EndDate
                };

                _budgetService.AddBudget(budget);
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "Une erreur s'est produite lors de l'ajout du budget.");
                return View(new Budget
                {
                    CategoryId = CategoryId,
                    UserId = UserId,
                    BudgetAmount = BudgetAmount,
                    StartDate = StartDate,
                    EndDate = EndDate
                });
            }
        }

        [HttpGet("Budget/GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var budget = _budgetService.GetBudgets().FirstOrDefault(b => b.BudgetId == id);
            if (budget == null)
            {
                return NotFound();
            }

            return Json(budget);
        }

        [HttpGet("Budget/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var budget = _budgetService.GetBudgets().FirstOrDefault(b => b.BudgetId == id);
            if (budget == null)
            {
                return NotFound();
            }
            var categories = _budgetService.GetCategories();
            ViewBag.Categories = categories;
            return View(budget);
        }

        [HttpPut("Budget/Update")]
        public IActionResult Update([FromBody] Budget budget)
        {
            try
            {
                _budgetService.UpdateBudget(budget);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return BadRequest(new { message = "Une erreur s'est produite lors de la modification du budget." });
            }
        }

        [HttpPost("Budget/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _budgetService.DeleteBudget(id);
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "Une erreur s'est produite lors de la suppression du budget.");
                return RedirectToAction("List");
            }
        }

        [HttpGet("Transaction/TransBudget/{CategoryId}/{StartDate}/{EndDate}")]
        public IActionResult ViewTransactionBudget(int CategoryId, long StartDate, long EndDate)
        {
            DateTimeOffset start = DateTimeOffset.FromUnixTimeMilliseconds(StartDate);
            DateTimeOffset end = DateTimeOffset.FromUnixTimeMilliseconds(EndDate);

            DateTime dateTimeStart = start.UtcDateTime;
            DateTime dateTimeEnd = end.UtcDateTime;

            try
            {
                var transactions = _budgetService.ViewTransactionBudget(CategoryId, dateTimeStart, dateTimeEnd);

                if (transactions == null || !transactions.Any())
                {
                    return View("transBudget", new List<Transaction>());
                }

                return View("transBudget", transactions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Une erreur s'est produite.");
            }
        }

    }
}
