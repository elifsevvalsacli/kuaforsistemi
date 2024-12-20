using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using odevkuafor.Models;

namespace odevkuafor.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin Paneli Ana Sayfası
        public IActionResult Index()
        {
            return View();
        }

        // Çalışan Listesi
        public async Task<IActionResult> Employees()
        {
            var employees = await _context.Employees.Include(e => e.EmployeeServices).ThenInclude(es => es.Service).ToListAsync();
            return View(employees);
        }

        // Hizmet Türleri Listesi
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services.Include(s => s.EmployeeServices).ThenInclude(es => es.Employee).ToListAsync();
            return View(services);
        }

        // Çalışan Ekleme (GET)
        [HttpGet]
        public IActionResult AddEmployee()
        {
            return View();
        }

        // Çalışan Ekleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Employees));
            }
            return View(employee);
        }

        // Hizmet Ekleme (GET)
        [HttpGet]
        public IActionResult AddService()
        {
            return View();
        }

        // Hizmet Ekleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Services));
            }
            return View(service);
        }

        // Çalışan-Hizmet Atama Sayfası (GET)
        [HttpGet]
        public async Task<IActionResult> AssignEmployeeToService()
        {
            ViewBag.Employees = await _context.Employees.ToListAsync();
            ViewBag.Services = await _context.Services.ToListAsync();
            return View();
        }

        // Çalışan-Hizmet Atama (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignEmployeeToService(int employeeId, int serviceId)
        {
            if (!_context.EmployeeServices.Any(es => es.EmployeeId == employeeId && es.ServiceId == serviceId))
            {
                var employeeService = new EmployeeService
                {
                    EmployeeId = employeeId,
                    ServiceId = serviceId
                };

                _context.EmployeeServices.Add(employeeService);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Employees));
        }

        // Çalışan Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Employees));
        }

        // Hizmet Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Services));
        }
    }
}
