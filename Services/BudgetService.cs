using gestion_budget.Models;
using Microsoft.Data.SqlClient;

namespace gestion_budget.Services
{
    public class BudgetService
    {
        private readonly string _connectionString;

        public BudgetService(string connectionString)
        {
            _connectionString = connectionString;
        }
        
        public List<Budget> GetBudgets()
        {
            var budgets = new List<Budget>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    @"SELECT b.BudgetId, b.UserId, b.CategoryId, b.BudgetAmount, b.StartDate, b.EndDate,
                             c.Name AS CategoryName, c.ParentCategoryId, c.IsIncome
                      FROM Budgets b
                      INNER JOIN Categories c ON b.CategoryId = c.CategoryId",
                    connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        budgets.Add(new Budget
                        {
                            BudgetId = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            CategoryId = reader.GetInt32(2),
                            BudgetAmount = reader.GetDecimal(3),
                            StartDate = reader.GetDateTime(4),
                            EndDate = reader.GetDateTime(5),
                            Category = new Category
                            {
                                CategoryId = reader.GetInt32(2),
                                Name = reader.GetString(6),
                                ParentCategoryId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                                IsIncome = reader.GetBoolean(8)
                            }
                        });
                    }
                }
            }
            return budgets;
        }

        public void AddBudget(Budget budget)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    "INSERT INTO Budgets (UserId, CategoryId, BudgetAmount, StartDate, EndDate) VALUES (@UserId, @CategoryId, @BudgetAmount, @StartDate, @EndDate)",
                    connection);

                command.Parameters.AddWithValue("@UserId", budget.UserId);
                command.Parameters.AddWithValue("@CategoryId", budget.CategoryId);
                command.Parameters.AddWithValue("@BudgetAmount", budget.BudgetAmount);
                command.Parameters.AddWithValue("@StartDate", budget.StartDate);
                command.Parameters.AddWithValue("@EndDate", budget.EndDate);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateBudget(Budget budget)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    @"UPDATE Budgets 
                      SET CategoryId = @CategoryId, BudgetAmount = @BudgetAmount, StartDate = @StartDate, EndDate = @EndDate
                      WHERE BudgetId = @BudgetId",
                    connection);

                command.Parameters.AddWithValue("@BudgetId", budget.BudgetId);
                command.Parameters.AddWithValue("@CategoryId", budget.CategoryId);
                command.Parameters.AddWithValue("@BudgetAmount", budget.BudgetAmount);
                command.Parameters.AddWithValue("@StartDate", budget.StartDate);
                command.Parameters.AddWithValue("@EndDate", budget.EndDate);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteBudget(int budgetId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    "DELETE FROM Budgets WHERE BudgetId = @BudgetId",
                    connection);

                command.Parameters.AddWithValue("@BudgetId", budgetId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<Category> GetCategories()
        {
            var categories = new List<Category>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT CategoryId, Name, IsIncome FROM Categories", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            CategoryId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            IsIncome = reader.GetBoolean(2)
                        });
                    }
                }
            }
            return categories;
        }

        public List<Transaction> ViewTransactionBudget(int parentCategoryId, DateTime startDate, DateTime endDate)
        {
            var transactions = new List<Transaction>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                SELECT 
                    T.TransactionId,
                    T.UserId,
                    T.CategoryId,
                    T.Amount,
                    T.TransactionDate,
                    T.Note
                FROM 
                    Transactions T
                WHERE 
                    T.CategoryId IN (
                        SELECT C.CategoryId
                        FROM Categories C
                        WHERE C.ParentCategoryId = @ParentCategoryId
                    )
                    AND T.TransactionDate BETWEEN @StartDate AND @EndDate
                ORDER BY 
                    T.TransactionDate;";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ParentCategoryId", parentCategoryId);
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var transaction = new Transaction
                            {
                                TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate")),
                                Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? null : reader.GetString(reader.GetOrdinal("Note"))
                            };

                            transactions.Add(transaction);
                        }
                    }
                }
            }
            return transactions;
        }
    }
}
