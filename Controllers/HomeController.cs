using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeamTrack.Services;
using TeamTrack.Models;

namespace TeamTrack.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseService _dbService;

        public HomeController(DatabaseService dbService)
        {
            _dbService = dbService;

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Employee()
        {
            return View();
        }
        public IActionResult Department()
        {
            return View();
        }
        public IActionResult ExecuteQuery()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ExecuteQuery(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    ViewBag.ErrorMessage = "Query cannot be empty.";
                    return View();
                }

                // Execute the query and get the result as a DataTable
                var result = _dbService.ExecuteQuery(query);

                // Pass the result to the view
                ViewBag.QueryResult = result;
                return View();
            }
            catch (Exception ex)
            {
                // Handle errors and display an error message
                ViewBag.ErrorMessage = $"Error executing query: {ex.Message}";
                return View();
            }
        }


        public IActionResult AddEmployee()
        {
            // Fetch departments and managers from the database
            ViewBag.Departments = _dbService.GetData("SELECT DepartmentId, DepartmentName FROM Department");
            ViewBag.Managers = _dbService.GetData("SELECT EmployeeId, CONCAT(FirstName, ' ', LastName) AS FullName FROM Employee");

            return View();
        }

        [HttpPost]
        public IActionResult AddEmployee(string firstName, string lastName, string email, string role, string[] departmentIds, string[] managerIds)
        {
            try
            {
                // Generate a unique EmployeeId
                var employeeId = _dbService.GenerateEmployeeId(firstName, lastName);

                // Insert the employee into the Employee table
                var query = $@"
            INSERT INTO Employee (EmployeeId, FirstName, LastName, Email, Role) 
            VALUES ('{employeeId}', '{firstName}', '{lastName}', '{email}', '{role}')";
                _dbService.InsertData(query);

                // Insert selected departments into the EmployeeDepartment table
                if (departmentIds != null && departmentIds.Length > 0)
                {
                    foreach (var departmentId in departmentIds)
                    {
                        var empDeptQuery = $@"
                    INSERT INTO EmployeeDepartment (EmployeeId, DepartmentId) 
                    VALUES ('{employeeId}', '{departmentId}')";
                        _dbService.InsertData(empDeptQuery);
                    }
                }

                // Insert selected managers into the Manager table
                if (managerIds != null && managerIds.Length > 0)
                {
                    foreach (var managerId in managerIds)
                    {
                        var managerQuery = $@"
                    INSERT INTO Manager (EmployeeId, ManagerId) 
                    VALUES ('{employeeId}', '{managerId}')";
                        _dbService.InsertData(managerQuery);
                    }
                }

                return RedirectToAction("DisplayEmployees");
            }
            catch (Exception ex)
            {
                // Log the error and return an error view if needed
                return BadRequest($"Error adding employee: {ex.Message}");
            }
        }


        public IActionResult DeleteEmployee(string id)
        {
            // Query to fetch the employee details
            var query = $"SELECT EmployeeId, FirstName, LastName, Email, Role FROM Employee WHERE EmployeeId = '{id}'";

            var employeeTable = _dbService.GetData(query);

            // Check if the employee exists
            if (employeeTable.Rows.Count == 0)
            {
                return NotFound("Employee not found.");
            }

            var employee = new Employee
            {
                EmployeeId = employeeTable.Rows[0]["EmployeeId"]?.ToString() ?? string.Empty,
                FirstName = employeeTable.Rows[0]["FirstName"]?.ToString() ?? string.Empty,
                LastName = employeeTable.Rows[0]["LastName"]?.ToString() ?? string.Empty,
                Email = employeeTable.Rows[0]["Email"]?.ToString() ?? string.Empty,
                Role = employeeTable.Rows[0]["Role"]?.ToString() ?? string.Empty,
            };

            return View(employee);
        }


        [HttpPost]
        public IActionResult DeleteEmployeeConfirmed(string employeeId)
        {
            try

            {
                // Delete related records from the Manager table
                var deleteManagerQuery = $"DELETE FROM Manager WHERE EmployeeId = '{employeeId}'";
                _dbService.InsertData(deleteManagerQuery);

                // Delete related records from the EmployeeDepartment table
                var deleteEmployeeDepartmentQuery = $"DELETE FROM EmployeeDepartment WHERE EmployeeId = '{employeeId}'";
                _dbService.InsertData(deleteEmployeeDepartmentQuery);

                // Delete the employee from the Employee table
                var deleteEmployeeQuery = $"DELETE FROM Employee WHERE EmployeeId = '{employeeId}'";
                _dbService.InsertData(deleteEmployeeQuery);

                return RedirectToAction("DisplayEmployees");
            }
            catch (Exception ex)
            {
                // Log the error and return an error view if needed
                return BadRequest($"Error deleting employee: {ex.Message}");
            }
        }

        public IActionResult DeleteDepartment(string id)
        {
            // Query to fetch the department details
            var query = $"SELECT DepartmentId, DepartmentName FROM Department WHERE DepartmentId = '{id}'";

            var departmentTable = _dbService.GetData(query);

            // Check if the department exists
            if (departmentTable.Rows.Count == 0)
            {
                return NotFound("Department not found.");
            }

            var department = new Department
            {
                DepartmentId = departmentTable.Rows[0]["DepartmentId"].ToString(),
                DepartmentName = departmentTable.Rows[0]["DepartmentName"].ToString()
            };

            return View(department);
        }


        [HttpPost]
        public IActionResult DeleteDepartmentConfirmed(string departmentId)
        {
            try
            {
                // Delete related records from the EmployeeDepartment table
                var deleteEmployeeDepartmentQuery = $"DELETE FROM EmployeeDepartment WHERE DepartmentId = '{departmentId}'";
                _dbService.InsertData(deleteEmployeeDepartmentQuery);

                // Delete the department from the Department table
                var deleteDepartmentQuery = $"DELETE FROM Department WHERE DepartmentId = '{departmentId}'";
                _dbService.InsertData(deleteDepartmentQuery);

                return RedirectToAction("DisplayDepartments");
            }
            catch (Exception ex)
            {
                // Log the error and return an error view if needed
                return BadRequest($"Error deleting department: {ex.Message}");
            }
        }
        public IActionResult AddDepartment()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddDepartment(string departmentName)
        {
            var departmentId = _dbService.GenerateDepartmentId(departmentName);
            var query = $"INSERT INTO Department (DepartmentId, DepartmentName) VALUES ('{departmentId}', '{departmentName}')";
            _dbService.InsertData(query);

            return RedirectToAction("DisplayDepartments");
        }

        public IActionResult DisplayEmployees()
        {
            var query = @"
       SELECT 
    e.EmployeeId, 
    e.FirstName, 
    e.LastName, 
    e.Email, 
    e.Role, 
    (
        SELECT STRING_AGG(d.DepartmentName, ', ')
        FROM EmployeeDepartment ed
        INNER JOIN Department d ON ed.DepartmentId = d.DepartmentId
        WHERE ed.EmployeeId = e.EmployeeId
    ) AS DepartmentName,
    (
        SELECT STRING_AGG(CONCAT(mng.FirstName, ' ', mng.LastName), ', ')
        FROM Manager m
        INNER JOIN Employee mng ON m.ManagerId = mng.EmployeeId
        WHERE m.EmployeeId = e.EmployeeId
    ) AS ManagerName
FROM Employee e";

            var employees = _dbService.GetData(query);
            return View(employees);
        }



        public IActionResult DisplayDepartments()
        {
            var query = "SELECT * FROM Department";
            var departments = _dbService.GetData(query);
            return View(departments);
        }

        public IActionResult EditDepartment(string id)
        {
            var query = $"SELECT * FROM Department WHERE DepartmentId = '{id}'";
            var department = _dbService.GetData(query);
            if (department.Rows.Count == 0)
            {
                // Handle the case where no employee is found
                return NotFound("Department not found.");
            }

            var Department = department.Rows[0];
            return View(Department);
        }

        [HttpPost]
        [HttpPost]
        public IActionResult EditDepartment(string departmentId, string departmentName)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(departmentId) || string.IsNullOrEmpty(departmentName))
                {
                    return BadRequest("All fields are required.");
                }

                // Extract the numeric part of the DepartmentId
                var numericPart = new string(departmentId.SkipWhile(c => !char.IsDigit(c)).ToArray());

                // Generate the new prefix using the updated department name
                var newPrefix = departmentName.Substring(0, 2).ToUpper();

                // Combine the new prefix with the numeric part
                var newDepartmentId = $"{newPrefix}{numericPart}";

                // Update the Department table with the new ID and name
                var updateDepartmentQuery = $@"
        UPDATE Department 
        SET DepartmentId = '{newDepartmentId}', DepartmentName = '{departmentName}' 
        WHERE DepartmentId = '{departmentId}'";

                _dbService.InsertData(updateDepartmentQuery);

                // Update the EmployeeDepartment table to reflect the new DepartmentId
                var updateEmployeeDepartmentQuery = $@"
        UPDATE EmployeeDepartment 
        SET DepartmentId = '{newDepartmentId}' 
        WHERE DepartmentId = '{departmentId}'";

                _dbService.InsertData(updateEmployeeDepartmentQuery);

                return RedirectToAction("DisplayDepartments");
            }
            catch (Exception ex)
            {
                // Log the error and return an error view if needed
                return BadRequest($"Error updating department: {ex.Message}");
            }
        }




        public IActionResult EditEmployee(string id)
        {
            // Query to fetch the employee details along with department and manager associations
            var query = $@"
SELECT e.EmployeeId, e.FirstName, e.LastName, e.Email, e.Role, 
       STRING_AGG(ed.DepartmentId, ',') AS DepartmentIds, 
       STRING_AGG(m.ManagerId, ',') AS ManagerIds
FROM Employee e
LEFT JOIN EmployeeDepartment ed ON e.EmployeeId = ed.EmployeeId
LEFT JOIN Manager m ON e.EmployeeId = m.EmployeeId
WHERE e.EmployeeId = '{id}'
GROUP BY e.EmployeeId, e.FirstName, e.LastName, e.Email, e.Role";
            var employeeTable = _dbService.GetData(query);

            // Check if the employee exists
            if (employeeTable.Rows.Count == 0)
            {
                return NotFound("Employee not found.");
            }

            // Get the first row (since the query returns one employee)
            var employeeRow = employeeTable.Rows[0];

            // Populate ViewBag with Departments and Managers
            ViewBag.Departments = _dbService.GetData("SELECT DepartmentId, DepartmentName FROM Department");
            ViewBag.Managers = _dbService.GetData($"SELECT EmployeeId, CONCAT(FirstName, ' ', LastName) AS FullName FROM Employee WHERE EmployeeId != '{id}'");

            // Pass the single DataRow to the view
            return View(employeeRow);
        }

        [HttpPost]


        [HttpPost]
        public IActionResult EditEmployee(string employeeId, string firstName, string lastName, string email, string role, string[] departmentIds, string[] managerIds)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(employeeId) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
                {
                    return BadRequest("All fields are required.");
                }

                // Extract the numeric part of the EmployeeId
                var numericPart = new string(employeeId.SkipWhile(c => !char.IsDigit(c)).ToArray());

                // Generate the new prefix using the updated first and last name
                var newPrefix = $"{firstName[0]}{lastName[0]}".ToUpper();

                // Combine the new prefix with the numeric part
                var newEmployeeId = $"{newPrefix}{numericPart}";

                // Update the Employee table with the new ID and details
                var updateEmployeeQuery = $@"
        UPDATE Employee 
        SET EmployeeId = '{newEmployeeId}', FirstName = '{firstName}', LastName = '{lastName}', Email = '{email}', Role = '{role}' 
        WHERE EmployeeId = '{employeeId}'";

                _dbService.InsertData(updateEmployeeQuery);

                // Update the EmployeeDepartment table
                // Step 1: Remove all existing department associations for the employee
                var deleteEmployeeDepartmentsQuery = $"DELETE FROM EmployeeDepartment WHERE EmployeeId = '{employeeId}'";
                _dbService.InsertData(deleteEmployeeDepartmentsQuery);

                // Step 2: Add back only the selected departments
                if (departmentIds != null && departmentIds.Length > 0)
                {
                    foreach (var departmentId in departmentIds)
                    {
                        var insertEmployeeDepartmentQuery = $@"
                INSERT INTO EmployeeDepartment (EmployeeId, DepartmentId) 
                VALUES ('{newEmployeeId}', '{departmentId}')";
                        _dbService.InsertData(insertEmployeeDepartmentQuery);
                    }
                }

                // Update the Manager table
                // Step 1: Remove all existing manager associations for the employee
                var deleteManagerQuery = $"DELETE FROM Manager WHERE EmployeeId = '{employeeId}'";
                _dbService.InsertData(deleteManagerQuery);

                // Step 2: Add back only the selected managers
                if (managerIds != null && managerIds.Length > 0)
                {
                    foreach (var managerId in managerIds)
                    {
                        var insertManagerQuery = $@"
                INSERT INTO Manager (EmployeeId, ManagerId) 
                VALUES ('{newEmployeeId}', '{managerId}')";
                        _dbService.InsertData(insertManagerQuery);
                    }
                }

                return RedirectToAction("DisplayEmployees");
            }
            catch (Exception ex)
            {
                // Log the error and return an error view if needed
                return BadRequest($"Error updating employee: {ex.Message}");
            }
        }
    }
}