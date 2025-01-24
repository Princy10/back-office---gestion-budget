using System.Security.Claims;
using System.Text;
using gestion_budget.DAL;
using gestion_budget.Models;
using gestion_budget.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace gestion_budget.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly TransactionService _transactionService;
        public TransactionController(AppDbContext context)
        {
            var connectionString = "Server=DESKTOP-7A266J8;Database=BudgetManagement;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";
            _transactionService = new TransactionService(connectionString);
            _context = context;
        }

        public IActionResult List(int page = 1, int pageSize = 10, int? categoryId = null, bool? isIncome = null, DateTime? startDate = null, DateTime? endDate = null, decimal? minAmount = null, decimal? maxAmount = null)
        {
            var transactions = _transactionService.GetTransactions(page, pageSize, categoryId, isIncome, startDate, endDate, minAmount, maxAmount);
            var totalTransactions = _transactionService.GetTotalTransactionsCount();
            var subcategories = _transactionService.GetSubCat();
            ViewBag.subcategories = subcategories ?? new List<Category>();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalTransactions / pageSize);
            ViewBag.CurrentPage = page;
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
        public IActionResult Add(int CategoryId, int? SubCategoryId, int UserId, decimal Amount, DateTime TransactionDate, string Note)
        {
            try
            {
                var transaction = new Transaction
                {
                    CategoryId = SubCategoryId ?? CategoryId,
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
                    CategoryId = SubCategoryId ?? CategoryId,
                    UserId = UserId,
                    Amount = Amount,
                    TransactionDate = TransactionDate,
                    Note = Note
                });
            }
        }


        [HttpGet("Transaction/GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var transaction = _transactionService.GetTransactionById(id);
            if (transaction == null)
            {
                return NotFound();
            }

            return Json(transaction);
        }

        [HttpGet("Transaction/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var transaction = _transactionService.GetTransactionById(id);
            if (transaction == null)
            {
                return NotFound();
            }

            var categories = _transactionService.GetSubCat();
            ViewBag.Categories = categories ?? new List<Category>();
            return View(transaction);
        }

        [HttpPut("Transaction/Update")]
        public IActionResult Update([FromBody] Transaction transaction)
        {
            try
            {
                _transactionService.UpdateTransaction(transaction);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return BadRequest(new { message = "Une erreur s'est produite lors de la modification de la transaction." });
            }
        }

        [HttpPost("Transaction/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _transactionService.DeleteTransaction(id);
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "Une erreur s'est produite lors de la suppression de la transaction.");
                return RedirectToAction("List");
            }
        }

        [HttpGet("Transaction/GetSubCategories/{id}")]
        public IActionResult GetSubCategories(int id)
        {
            var subCategories = _transactionService.GetSubCatByIdCat(id);
            return Json(subCategories);
        }

        public IActionResult Export(int? categoryId = null, bool? isIncome = null, DateTime? startDate = null, DateTime? endDate = null, decimal? minAmount = null, decimal? maxAmount = null)
        {
            var transactions = _transactionService.GetTransactions(1, int.MaxValue, categoryId, isIncome, startDate, endDate, minAmount, maxAmount);

            var csv = new StringBuilder();
            csv.AppendLine("TransactionId,Category,Type,Amount,Date,Note");

            foreach (var transaction in transactions)
            {
                var type = transaction.Category.IsIncome ? "Revenu" : "Dépense";
                var amount = transaction.Category.IsIncome ? $"+{transaction.Amount:N0}" : $"-{transaction.Amount:N0}";
                csv.AppendLine($"{transaction.TransactionId},{transaction.Category.Name},{type},{amount},{transaction.TransactionDate:dd-MM-yyyy HH:mm},{transaction.Note}");
            }

            var fileName = "transactions_export.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        public IActionResult ExportExcel(int? categoryId = null, bool? isIncome = null, DateTime? startDate = null, DateTime? endDate = null, decimal? minAmount = null, decimal? maxAmount = null)
        {
            var transactions = _transactionService.GetTransactions(1, int.MaxValue, categoryId, isIncome, startDate, endDate, minAmount, maxAmount);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Transactions");
                worksheet.Cells[1, 1].Value = "TransactionId";
                worksheet.Cells[1, 2].Value = "Category";
                worksheet.Cells[1, 3].Value = "Type";
                worksheet.Cells[1, 4].Value = "Amount";
                worksheet.Cells[1, 5].Value = "Date";
                worksheet.Cells[1, 6].Value = "Note";

                int row = 2;
                foreach (var transaction in transactions)
                {
                    var type = transaction.Category.IsIncome ? "Revenu" : "Dépense";
                    var amount = transaction.Category.IsIncome ? $"+{transaction.Amount:N0}" : $"-{transaction.Amount:N0}";
                    worksheet.Cells[row, 1].Value = transaction.TransactionId;
                    worksheet.Cells[row, 2].Value = transaction.Category.Name;
                    worksheet.Cells[row, 3].Value = type;
                    worksheet.Cells[row, 4].Value = amount;
                    worksheet.Cells[row, 5].Value = transaction.TransactionDate.ToString("dd-MM-yyyy HH:mm");
                    worksheet.Cells[row, 6].Value = transaction.Note;
                    row++;
                }

                var fileContents = package.GetAsByteArray();
                var fileName = "transactions_export.xlsx";
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
        [HttpPost]
        public IActionResult UploadCsv(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                return Json(new { success = false, message = "Veuillez sélectionner un fichier CSV valide." });
            }

            var uploadedData = new List<string[]>();
            try
            {
                using (var reader = new StreamReader(csvFile.OpenReadStream()))
                {
                    var lines = reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var line in lines.Skip(1)) // Ignorer l'en-tête
                    {
                        var values = line.Split(',', StringSplitOptions.TrimEntries);
                        uploadedData.Add(values);
                    }
                }

                return Json(new
                {
                    success = true,
                    message = $"{uploadedData.Count} lignes extraites avec succès.",
                    rows = uploadedData
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de l'extraction : {ex.Message}" });
            }
        }

        [HttpPost]
        public JsonResult InsertData([FromBody] DataPayload payload)
        {
            if (payload == null || payload.Data == null || payload.Data.Count == 0)
            {
                return Json(new { success = false, message = "Aucune donnée à traiter." });
            }

            var transactions = new List<Transaction>();
            foreach (var row in payload.Data)
            {
                var category = _context.Categories
                    .FirstOrDefault(c => c.Name == row[0].Trim());

                if (category == null)
                {
                    Console.WriteLine($"Catégorie non trouvée : {row[0]}");
                    continue;
                }
                Console.WriteLine($"Categorie: {row[0]}, ");

                var transaction = new Transaction
                {
                    CategoryId = category.CategoryId,
                    UserId = int.Parse(payload.UserId),
                    Amount = decimal.Parse(row[1]),
                    TransactionDate = DateTime.Parse(row[2]),
                    Note = row[3]
                };

                transactions.Add(transaction);

                Console.WriteLine($"UserId: {transaction.UserId}, " +
                    $"Categorie: {row[0]}, " +
                    $"CategoryId: {transaction.CategoryId}, " +
                    $"Montant: {transaction.Amount}, " +
                    $"Date: {transaction.TransactionDate}");
            }

            foreach (var transaction in transactions)
            {
                _transactionService.AddTransaction(transaction);
            }

            return Json(new
            {
                success = true,
                message = $"{transactions.Count} transactions ajoutées avec succès."
            });
        }
        public class DataPayload
        {
            public string UserId { get; set; }
            public List<List<string>> Data { get; set; }
        }
    }
}
