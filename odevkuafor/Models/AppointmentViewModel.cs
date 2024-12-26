
using odevkuafor.Models;

public class AppointmentViewModel
{
    public string CustomerName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int EmployeeId { get; set; }
    public int ServiceId { get; set; }
    public List<Employee> Employees { get; set; } = new List<Employee>(); // Null olmayan başlangıç değeri
    public List<Service> Services { get; set; } = new List<Service>(); // Null olmayan başlangıç değeri
}