namespace TeamTrack.Models
{
    public class Employee
    {
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string? DepartmentId { get; set; } // Nullable to avoid null reference issues
        public string? ManagerId { get; set; }

    }
}
