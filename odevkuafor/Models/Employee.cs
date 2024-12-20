using System.Collections.Generic;

namespace odevkuafor.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty; // Çalışan adı

        public string Specialization { get; set; } = string.Empty; // Uzmanlık alanı

        // Many-to-Many ilişki için navigation property
        public ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();
    }
}
