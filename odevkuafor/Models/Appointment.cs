using System;
using System.Collections.Generic;
    namespace odevkuafor.Models
{
    public class Appointment
    {
        private DateTime _appointmentDate;

        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty; // Varsayılan değer

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

        // İlişkiler
        public int ServiceId { get; set; } // Hizmet ID'si
        public Service? Service { get; set; } // Hizmet ilişkisi
        public string ServiceType => Service?.ServiceType ?? string.Empty; // Hizmet tipi

        public int EmployeeId { get; set; } // Çalışan ID'si
        public Employee? Employee { get; set; } // Çalışan ilişkisi
        public string BarberName => Employee?.Name ?? string.Empty; // Çalışan adı
    }
}
