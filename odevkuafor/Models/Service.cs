using System.Collections.Generic;
using odevkuafor.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace odevkuafor.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Hizmet adı
        public string ServiceType { get; set; } = string.Empty; // Hizmet türü
        public decimal Price { get; set; } // Hizmet ücreti
        public int DurationInMinutes { get; set; } // Dakika cinsinden işlem süresi

        public ICollection<Appointment> Appointments { get; set; }

        // Many-to-Many ilişki için navigation property
        public ICollection<EmployeeService> EmployeeServices { get; set; } = new HashSet<EmployeeService>();
    }
}

