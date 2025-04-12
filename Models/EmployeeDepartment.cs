using TeamTrack.Models;

public class EmployeeDepartment
{
    public string EmployeeId { get; set; }
    public Employee Employee { get; set; }
    public string DepartmentId { get; set; }
    public Department Department { get; set; }
}