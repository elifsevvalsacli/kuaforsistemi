using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace odevkuafor.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ücret zorunludur")]
        public decimal Price { get; set; }

       
        public string ServiceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Süre zorunludur")]
        public int DurationInMinutes { get; set; }

        public virtual ICollection<Appointment>? Appointments { get; set; }
        public virtual ICollection<EmployeeService> EmployeeServices { get; set; } = new HashSet<EmployeeService>();
    }
}