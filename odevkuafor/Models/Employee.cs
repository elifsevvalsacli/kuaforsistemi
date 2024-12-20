using System.Collections.Generic;

namespace odevkuafor.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();

        // Yardımcı özellik
        public IEnumerable<Service> Services => EmployeeServices.Select(es => es.Service);
    }
}
