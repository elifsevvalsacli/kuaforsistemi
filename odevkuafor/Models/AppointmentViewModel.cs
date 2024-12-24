namespace odevkuafor.Models
{
    public class AppointmentViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public List<Service> Services { get; set; } = new List<Service>();
    }
}
