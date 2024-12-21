using System;
using System.Collections.Generic;
using System.Linq;

namespace odevkuafor.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;

        // Çalışanın yaptığı hizmetler ilişkisi
        public ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();

        // Çalışanın yaptığı hizmetleri döndüren özellik
        public IEnumerable<Service> Services => EmployeeServices.Select(es => es.Service);
    }
}
