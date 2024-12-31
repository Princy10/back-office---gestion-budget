using gestion_budget.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace gestion_budget.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly string _connectionString = "Server=DESKTOP-7A266J8;Database=BudgetManagement;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";

        // Liste des transactions
        public IActionResult List()
        {
            var transactions = new List<Transaction>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    @"SELECT t.TransactionId, t.UserId, t.CategoryId, t.Amount, t.TransactionDate, t.Note, 
                             c.Name AS CategoryName, c.ParentCategoryId
                      FROM Transactions t
                      INNER JOIN Categories c ON t.CategoryId = c.CategoryId",
                    connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            TransactionId = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            CategoryId = reader.GetInt32(2),
                            Amount = reader.GetDecimal(3),
                            TransactionDate = reader.GetDateTime(4),
                            Note = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Category = new Category
                            {
                                CategoryId = reader.GetInt32(2),
                                Name = reader.GetString(6),
                                ParentCategoryId = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                            }
                        });
                    }
                }
            }
            return View(transactions);
        }

        // Ajouter une transaction
        public IActionResult Add()
        {
            ViewBag.Categories = GetCategories();
            return View();
        }

        [HttpPost]
        public IActionResult Add(Transaction transaction)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = GetCategories();
                return View(transaction);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    "INSERT INTO Transactions (UserId, CategoryId, Amount, TransactionDate, Note) VALUES (@UserId, @CategoryId, @Amount, @TransactionDate, @Note)",
                    connection);
                command.Parameters.AddWithValue("@UserId", transaction.UserId);
                command.Parameters.AddWithValue("@CategoryId", transaction.CategoryId);
                command.Parameters.AddWithValue("@Amount", transaction.Amount);
                command.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
                command.Parameters.AddWithValue("@Note", transaction.Note ?? (object)DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
            return RedirectToAction("List");
        }

        // Modifier une transaction
        public IActionResult Edit(int id)
        {
            var transaction = GetTransactionById(id);
            if (transaction == null)
                return NotFound();

            ViewBag.Categories = GetCategories();
            return View(transaction);
        }

        [HttpPost]
        public IActionResult Edit(Transaction transaction)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = GetCategories();
                return View(transaction);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    "UPDATE Transactions SET CategoryId = @CategoryId, Amount = @Amount, TransactionDate = @TransactionDate, Note = @Note WHERE TransactionId = @TransactionId",
                    connection);
                command.Parameters.AddWithValue("@TransactionId", transaction.TransactionId);
                command.Parameters.AddWithValue("@CategoryId", transaction.CategoryId);
                command.Parameters.AddWithValue("@Amount", transaction.Amount);
                command.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
                command.Parameters.AddWithValue("@Note", transaction.Note ?? (object)DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
            return RedirectToAction("List");
        }

        // Supprimer une transaction
        [HttpPost]
        public IActionResult Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    "DELETE FROM Transactions WHERE TransactionId = @TransactionId",
                    connection);
                command.Parameters.AddWithValue("@TransactionId", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
            return RedirectToAction("List");
        }

        // Méthode pour récupérer une transaction par ID
        private Transaction GetTransactionById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    @"SELECT t.TransactionId, t.UserId, t.CategoryId, t.Amount, t.TransactionDate, t.Note, 
                             c.Name AS CategoryName
                      FROM Transactions t
                      INNER JOIN Categories c ON t.CategoryId = c.CategoryId
                      WHERE t.TransactionId = @TransactionId",
                    connection);
                command.Parameters.AddWithValue("@TransactionId", id);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Transaction
                        {
                            TransactionId = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            CategoryId = reader.GetInt32(2),
                            Amount = reader.GetDecimal(3),
                            TransactionDate = reader.GetDateTime(4),
                            Note = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Category = new Category
                            {
                                Name = reader.GetString(6)
                            }
                        };
                    }
                }
            }
            return null;
        }

        // Méthode pour récupérer les catégories
        private List<Category> GetCategories()
        {
            var categories = new List<Category>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT CategoryId, Name FROM Categories", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            CategoryId = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return categories;
        }
    }
}