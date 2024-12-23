using System.Collections.Generic;

namespace odevkuafor.Models
{
    public class Service
    {
        public int Id { get; set; }

        public string ServiceType { get; set; } = string.Empty; // Hizmet türü

        public decimal Price { get; set; } // Hizmet ücreti

        public string Name { get; set; } = string.Empty; // Hizmet adı

        // Hizmetin hangi çalışana ait olduğunu belirtir
        public List<Employee> Employees { get; set; } = new(); // Daha modern bir yazım

        // Many-to-Many ilişki için navigation property
        public ICollection<EmployeeService> EmployeeServices { get; set; } = new HashSet<EmployeeService>();
    }
}

