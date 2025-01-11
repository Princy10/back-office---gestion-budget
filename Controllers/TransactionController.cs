﻿using System.Security.Claims;
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

            var categories = _transactionService.GetCategories();
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
    }
}
