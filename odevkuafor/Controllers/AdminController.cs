using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using odevkuafor.Models;

namespace odevkuafor.Controllers
{
    [Authorize(Roles = "Admin")]

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
            var employees = await _context.Employees
                .Include(e => e.EmployeeServices)
                .ThenInclude(es => es.Service)
                .ToListAsync();
            return View(employees);
        }

        // Hizmet Türleri Listesi
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services
                .Include(s => s.EmployeeServices)
                .ThenInclude(es => es.Employee)
                .ToListAsync();
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
        [HttpGet]
        public async Task<IActionResult> AddService()
        {
            ViewBag.Employees = await _context.Employees.ToListAsync();
            return View();
        }

        // Hizmet Ekleme (POST) - mevcut metodunuz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(Service service, int[] employeeIds)
        {
            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                if (employeeIds != null && employeeIds.Length > 0)
                {
                    foreach (var employeeId in employeeIds)
                    {
                        var employeeService = new EmployeeService
                        {
                            EmployeeId = employeeId,
                            ServiceId = service.Id
                        };
                        _context.EmployeeServices.Add(employeeService);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Hizmet başarıyla eklendi.";
                return RedirectToAction(nameof(Services));
            }

            ViewBag.Employees = await _context.Employees.ToListAsync();
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


        // Hizmet Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                // İlgili hizmeti ve ilişkili kayıtları getir
                var service = await _context.Services
                    .Include(s => s.EmployeeServices)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (service != null)
                {
                    // Önce ilişkili EmployeeServices kayıtlarını sil
                    if (service.EmployeeServices != null)
                    {
                        _context.EmployeeServices.RemoveRange(service.EmployeeServices);
                    }

                    // Varsa ilişkili randevuları kontrol et ve sil
                    var relatedAppointments = await _context.Appointments
                        .Where(a => a.ServiceId == id)
                        .ToListAsync();

                    if (relatedAppointments.Any())
                    {
                        _context.Appointments.RemoveRange(relatedAppointments);
                    }

                    // Son olarak hizmeti sil
                    _context.Services.Remove(service);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Hizmet başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Hizmet bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hizmet silinirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Services));
        }

        // Ayrıca DeleteEmployee metodunu da benzer şekilde güncelleyelim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                var employee = await _context.Employees
                    .Include(e => e.EmployeeServices)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (employee != null)
                {
                    // Önce ilişkili EmployeeServices kayıtlarını sil
                    if (employee.EmployeeServices != null)
                    {
                        _context.EmployeeServices.RemoveRange(employee.EmployeeServices);
                    }

                    // Varsa ilişkili randevuları kontrol et ve sil
                    var relatedAppointments = await _context.Appointments
                        .Where(a => a.EmployeeId == id)
                        .ToListAsync();

                    if (relatedAppointments.Any())
                    {
                        _context.Appointments.RemoveRange(relatedAppointments);
                    }

                    // Son olarak çalışanı sil
                    _context.Employees.Remove(employee);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Çalışan başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Çalışan bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Çalışan silinirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Employees));
        }
    }
}
