using System.Linq;  // ToList() metodunu kullanabilmek için

namespace odevkuafor.Models
{
    public class Appointment
    {
        private DateTime _appointmentDate;

        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public DateTime AppointmentDate
        {
            get => _appointmentDate;
            set
            {
                if (value.Kind == DateTimeKind.Unspecified)
                {
                    _appointmentDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                }
                else
                {
                    _appointmentDate = value.ToUniversalTime();
                }
            }
        }

        // Hizmet ve çalışan ilişkisi
        public int ServiceId { get; set; }
        public Service? Service { get; set; }
        public string ServiceType => Service?.ServiceType ?? string.Empty;

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public string BarberName => Employee?.Name ?? string.Empty;

        // Çalışanlara atanmış hizmetleri göstermek
        public List<Service> GetEmployeeServices()
        {
            return Employee?.Services.ToList() ?? new List<Service>();
        }
    }
}
