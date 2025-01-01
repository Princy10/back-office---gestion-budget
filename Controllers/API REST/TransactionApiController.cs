using gestion_budget.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[ApiController]
[Route("api/transactions")]
public class TransactionApiController : ControllerBase
{
    private readonly string _connectionString = "Server=DESKTOP-7A266J8;Database=BudgetManagement;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";

    [HttpGet("{userId}")]
    public IActionResult GetTransactions(int userId)
    {
        var transactions = new List<Transaction>();
        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(
                @"SELECT t.TransactionId, t.UserId, t.CategoryId, t.Amount, t.TransactionDate, t.Note, 
                         c.Name AS CategoryName, c.ParentCategoryId 
                  FROM Transactions t
                  INNER JOIN Categories c ON t.CategoryId = c.CategoryId
                  WHERE t.UserId = @UserId",
                connection);
            command.Parameters.AddWithValue("@UserId", userId);

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
        return Ok(transactions);
    }

    [HttpPost]
    public IActionResult AddTransaction(Transaction transaction)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(
                "INSERT INTO Transactions (UserId, CategoryId, Amount, TransactionDate, Note) VALUES (@UserId, @CategoryId, @Amount, CONVERT(DATETIME, @TransactionDate,120), @Note)",
                connection);
            command.Parameters.AddWithValue("@UserId", transaction.UserId);
            command.Parameters.AddWithValue("@CategoryId", transaction.CategoryId);
            command.Parameters.AddWithValue("@Amount", transaction.Amount);
            command.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
            command.Parameters.AddWithValue("@Note", transaction.Note ?? (object)DBNull.Value);

            connection.Open();
            command.ExecuteNonQuery();
        }
        return Ok("Transaction added successfully");
    }

    [HttpPut("{transactionId}")]
    public IActionResult UpdateTransaction(int transactionId, Transaction transaction)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(
                "UPDATE Transactions SET CategoryId = @CategoryId, Amount = @Amount, TransactionDate = @TransactionDate, Note = @Note WHERE TransactionId = @TransactionId",
                connection);
            command.Parameters.AddWithValue("@TransactionId", transactionId);
            command.Parameters.AddWithValue("@CategoryId", transaction.CategoryId);
            command.Parameters.AddWithValue("@Amount", transaction.Amount);
            command.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
            command.Parameters.AddWithValue("@Note", transaction.Note ?? (object)DBNull.Value);

            connection.Open();
            command.ExecuteNonQuery();
        }
        return Ok("Transaction updated successfully");
    }

    [HttpDelete("{transactionId}")]
    public IActionResult DeleteTransaction(int transactionId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand("DELETE FROM Transactions WHERE TransactionId = @TransactionId", connection);
            command.Parameters.AddWithValue("@TransactionId", transactionId);

            connection.Open();
            command.ExecuteNonQuery();
        }
        return Ok("Transaction deleted successfully");
    }

    [HttpGet("categories")]
    public IActionResult GetCategories()
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
        return Ok(categories);
    }

}