using gestion_budget.Models;
using Microsoft.Data.SqlClient;

namespace gestion_budget.Services
{
    public class TransactionService
    {
        private readonly string _connectionString;

        public TransactionService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Transaction> GetTransactions()
        {
            var transactions = new List<Transaction>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    @"SELECT t.TransactionId, t.UserId, t.CategoryId, t.Amount, t.TransactionDate, t.Note, 
                             c.Name AS CategoryName, c.ParentCategoryId, c.IsIncome
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
            return transactions;
        }

        public void AddTransaction(Transaction transaction)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    "INSERT INTO Transactions (UserId, CategoryId, Amount, TransactionDate, Note) VALUES (@UserId, @CategoryId, @Amount, @TransactionDate, @Note)",
                    connection);

                command.Parameters.AddWithValue("@UserId", transaction.UserId);
                command.Parameters.AddWithValue("@CategoryId", transaction.CategoryId);
                command.Parameters.AddWithValue("@Amount", transaction.Amount);

                DateTime minSqlDate = new DateTime(1753, 1, 1);
                DateTime transactionDate = transaction.TransactionDate < minSqlDate ? minSqlDate : transaction.TransactionDate;

                command.Parameters.AddWithValue("@TransactionDate", transactionDate);
                command.Parameters.AddWithValue("@Note", transaction.Note ?? (object)DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public void UpdateTransaction(Transaction transaction)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    @"UPDATE Transactions 
                      SET UserId = @UserId, CategoryId = @CategoryId, Amount = @Amount, TransactionDate = @TransactionDate, Note = @Note
                      WHERE TransactionId = @TransactionId",
                    connection);

                command.Parameters.AddWithValue("@TransactionId", transaction.TransactionId);
                command.Parameters.AddWithValue("@UserId", transaction.UserId);
                command.Parameters.AddWithValue("@CategoryId", transaction.CategoryId);
                command.Parameters.AddWithValue("@Amount", transaction.Amount);

                DateTime minSqlDate = new DateTime(1753, 1, 1);
                DateTime transactionDate = transaction.TransactionDate < minSqlDate ? minSqlDate : transaction.TransactionDate;

                command.Parameters.AddWithValue("@TransactionDate", transactionDate);
                command.Parameters.AddWithValue("@Note", transaction.Note ?? (object)DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public void DeleteTransaction(int transactionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    "DELETE FROM Transactions WHERE TransactionId = @TransactionId",
                    connection);

                command.Parameters.AddWithValue("@TransactionId", transactionId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public List<Category> GetCategories()
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
