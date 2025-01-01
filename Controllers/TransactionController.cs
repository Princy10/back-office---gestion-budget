using System.Security.Claims;
using gestion_budget.Models;
using gestion_budget.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gestion_budget.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly TransactionService _transactionService;

        public TransactionController()
        {
            var connectionString = "Server=DESKTOP-7A266J8;Database=BudgetManagement;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";
            _transactionService = new TransactionService(connectionString);
        }

        public IActionResult List()
        {
            var transactions = _transactionService.GetTransactions();
            return View(transactions);
        }

        public IActionResult Add()
        {
            var categories = _transactionService.GetCategories();
            ViewBag.Categories = categories ?? new List<Category>();
            ViewBag.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return View();
        }

        [HttpPost]
        public IActionResult Add(int CategoryId, int UserId, decimal Amount, DateTime TransactionDate, string Note)
        {
            try
            {
                var transaction = new Transaction
                {
                    CategoryId = CategoryId,
                    UserId = UserId,
                    Amount = Amount,
                    TransactionDate = TransactionDate,
                    Note = Note
                };

                _transactionService.AddTransaction(transaction);
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "Une erreur s'est produite lors de l'insertion de la transaction.");
                return View(new Transaction
                {
                    CategoryId = CategoryId,
                    UserId = UserId,
                    Amount = Amount,
                    TransactionDate = TransactionDate,
                    Note = Note
                });
            }
        }

    }
}
