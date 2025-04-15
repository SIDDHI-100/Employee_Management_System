using System;
using System.Data;
using Microsoft.Data.SqlClient;
namespace TeamTrack.Services
{
    public class DatabaseService
    {
        private static DatabaseService _instance; // Singleton instance
        private static readonly object _lock = new object(); // Lock for thread safety
        private readonly string _connectionString;

        // Private constructor to prevent instantiation
        private DatabaseService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // Public static method to get the singleton instance
        public static DatabaseService GetInstance(string connectionString)
        {
            if (_instance == null)
            {
                lock (_lock) // Ensure thread safety
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseService(connectionString);
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// Executes a SELECT query and returns the result as a DataTable.
        /// </summary>
        public DataTable GetData(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                var dataTable = new DataTable();
                try
                {
                    adapter.Fill(dataTable);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error executing GetData query.", ex);
                }
                return dataTable;
            }
        }

        /// <summary>
        /// Executes an INSERT, UPDATE, or DELETE query.
        /// </summary>
        public void InsertData(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error executing InsertData query.", ex);
                }
            }
        }

        /// <summary>
        /// Generates a unique Employee ID using a stored procedure.
        /// </summary>
        public string GenerateEmployeeId(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("First name and last name cannot be null or empty.");

            string initials = $"{firstName[0]}{lastName[0]}".ToUpper();
            string query = $"SELECT TOP 1 EmployeeId FROM Employee ORDER BY EmployeeId DESC";

            int nextNumber = 1;

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                try
                {
                    connection.Open();
                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        string lastId = result.ToString();
                        string numericPart = lastId.Substring(initials.Length);
                        if (int.TryParse(numericPart, out int currentNumber))
                        {
                            nextNumber = currentNumber + 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error generating Employee ID.", ex);
                }
            }

            return $"{initials}{nextNumber:D3}";
        }

        public string GenerateDepartmentId(string deptName)
        {
            if (string.IsNullOrWhiteSpace(deptName))
                throw new ArgumentException("Department name cannot be null or empty.", nameof(deptName));

            string initials = deptName.Length >= 2
                ? $"{deptName[0]}{deptName[1]}".ToUpper()
                : $"{deptName[0]}X".ToUpper();

            string query = $"SELECT TOP 1 DepartmentId FROM Department ORDER BY DepartmentId DESC";

            int nextNumber = 1;

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                try
                {
                    connection.Open();
                    var result = command.ExecuteScalar();

                    if (result != DBNull.Value && result != null)
                    {
                        string lastId = result.ToString();
                        string numericPart = lastId.Substring(initials.Length);

                        if (int.TryParse(numericPart, out int currentNumber))
                        {
                            nextNumber = currentNumber + 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error generating Department ID.", ex);
                }
            }

            return $"{initials}{nextNumber:D3}";
        }

        public DataTable ExecuteQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            var dataTable = new DataTable();

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                try
                {
                    connection.Open();
                    adapter.Fill(dataTable);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error executing query.", ex);
                }
            }

            return dataTable;
        }
    }
}