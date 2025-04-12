using System;
using System.Data;
using System.Data.SqlClient;

namespace TeamTrack.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string ConnectionString)
        {
            _connectionString = ConnectionString ?? throw new ArgumentNullException(nameof(ConnectionString));
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
        /// </summary>newwww
        /// 

        public string GenerateEmployeeId(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("First name and last name cannot be null or empty.");

            // Generate initials based on the first letter of the first name and last name
            string initials = $"{firstName[0]}{lastName[0]}".ToUpper();

            // Query to get the last inserted EmployeeId with the same initials
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
                        // Extract the numeric part of the ID and increment it
                        string lastId = result.ToString();
                        string numericPart = lastId.Substring(initials.Length); // Skip the initials
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

            // Append the next number (formatted as a 3-digit number) to the initials
            return $"{initials}{nextNumber:D3}";
        }



        public string GenerateDepartmentId(string deptName)
        {
            if (string.IsNullOrWhiteSpace(deptName))
                throw new ArgumentException("Department name cannot be null or empty.", nameof(deptName));

            // Generate initials based on the first two letters of the department name
            string initials = deptName.Length >= 2
                ? $"{deptName[0]}{deptName[1]}".ToUpper()
                : $"{deptName[0]}X".ToUpper();

            // Query to get the last inserted DepartmentId with the same initials
            string query = $"SELECT TOP 1 DepartmentId FROM Department ORDER BY DepartmentId DESC";

            int nextNumber = 1;

            System.Console.WriteLine("Hello World!");
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                try
                {
                    connection.Open();
                    var result = command.ExecuteScalar();
                    Console.WriteLine($"Res {result}");

                    if (result != DBNull.Value && result != null)
                    {
                        // Extract the numeric part of the ID and increment it
                        string lastId = result.ToString();

                        Console.WriteLine("LastId", lastId);

                        string numericPart = lastId.Substring(initials.Length); // Skip the initials

                        Console.WriteLine(numericPart);

                        if (int.TryParse(numericPart, out int currentNumber))
                        {
                            nextNumber = currentNumber + 1;
                        }

                        Console.WriteLine(nextNumber);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error generating Department ID.", ex);
                }
            }

            Console.WriteLine($"{initials}{nextNumber:D3}");

            // Append the next number (formatted as a 3-digit number) to the initials
            return $"{initials}{nextNumber:D3}";
        }

     


    }
}