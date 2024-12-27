
using odevkuafor.Models;

public class AppointmentViewModel
{
    public string CustomerName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string AppointmentTime { get; set; }
    public int EmployeeId { get; set; }
    public int ServiceId { get; set; }
    public List<Employee> Employees { get; set; } = new List<Employee>();
    public List<Service> Services { get; set; } = new List<Service>();
}