﻿using System.Linq; // ToList() metodunu kullanabilmek için

namespace odevkuafor.Models
{
    public class Appointment
    {
        private DateTimeOffset _appointmentDate;

        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public DateTimeOffset AppointmentDate
        {
            get => _appointmentDate;
            set
            {
                // DateTimeOffset değeri doğrudan atanır, UTC'ye dönüştürmeye gerek yok
                _appointmentDate = value;
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
