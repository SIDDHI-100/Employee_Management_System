using TeamTrack.Models;

public class Manager
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }
    public int managerId { get; set; }
    public Employee ManagerEmployee { get; set; } // Renamed to avoid conflict
}