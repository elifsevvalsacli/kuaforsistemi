using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using odevkuafor.Models;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Admin Paneli Ana Sayfası
    public async Task<IActionResult> Index()
    {
        ViewBag.Appointments = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Employee)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
        return View();
    }
    // Çalışan Listesi
    public async Task<IActionResult> Employees()
    {
        ViewBag.Services = await _context.Services.ToListAsync(); // Servisleri ViewBag'e ekle
        var employees = await _context.Employees
            .Include(e => e.EmployeeServices) // Include to load related EmployeeServices
            .ThenInclude(es => es.Service)   // Include related Services
            .ToListAsync();

        return View(employees);
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

    // Çalışan Silme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        try
        {
            var employee = await _context.Employees
                .Include(e => e.EmployeeServices) // Load EmployeeServices
                .ThenInclude(es => es.Service)    // Also load related services
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                TempData["Error"] = "Çalışan bulunamadı.";
                return RedirectToAction(nameof(Employees));
            }

            // Remove EmployeeServices relationship entries first
            if (employee.EmployeeServices.Any())
            {
                _context.EmployeeServices.RemoveRange(employee.EmployeeServices);
            }

            // Remove the employee
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Çalışan başarıyla silindi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Silme işlemi başarısız: " + ex.Message;
        }

        return RedirectToAction(nameof(Employees));
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
            TempData["Success"] = "Hizmet başarıyla eklendi.";
            return RedirectToAction(nameof(Services));
        }
        return View(service);
    }

    // Hizmetler Listesi
    public async Task<IActionResult> Services()
    {
        var services = await _context.Services.ToListAsync();
        return View(services);
    }

    // Hizmet Silme (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteService(int id)
    {
        try
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                TempData["Error"] = "Hizmet bulunamadı.";
                return RedirectToAction(nameof(Services));
            }

            // Remove the service
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hizmet başarıyla silindi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Silme işlemi başarısız: " + ex.Message;
        }

        return RedirectToAction(nameof(Services));
    }

    // Hizmet Atama Sayfası (GET)
    [HttpGet]
    public async Task<IActionResult> AssignServiceToEmployee(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null)
        {
            TempData["Error"] = "Çalışan bulunamadı.";
            return RedirectToAction(nameof(Employees));
        }

        ViewBag.Employee = employee;
        ViewBag.Services = await _context.Services.ToListAsync();

        return View();
    }
    // Randevuları görüntüleme
    [HttpGet("Appointments")]
    public async Task<IActionResult> Appointments()
    {
        var appointments = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Employee)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return View(appointments);
    }

    // Randevu onaylama
    [HttpPost("ApproveAppointment/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            TempData["Error"] = "Randevu bulunamadı.";
            return RedirectToAction(nameof(Appointments));
        }

        appointment.IsApproved = true;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Randevu onaylandı.";

        return RedirectToAction(nameof(Appointments));
    }

    // Randevu silme
    [HttpPost("DeleteAppointment/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            TempData["Error"] = "Randevu bulunamadı.";
            return RedirectToAction(nameof(Appointments));
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Randevu silindi.";

        return RedirectToAction(nameof(Appointments));
    }
    // Hizmet Atama (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignServiceToEmployee(int employeeId, int[] serviceIds)
    {
        if (serviceIds != null && serviceIds.Length > 0)
        {
            foreach (var serviceId in serviceIds)
            {
                if (!_context.EmployeeServices.Any(es => es.EmployeeId == employeeId && es.ServiceId == serviceId))
                {
                    var employeeService = new EmployeeService
                    {
                        EmployeeId = employeeId,
                        ServiceId = serviceId
                    };

                    _context.EmployeeServices.Add(employeeService);
                }
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Hizmet başarıyla atandı.";
        }
        else
        {
            TempData["Error"] = "Hizmet seçilmedi.";
        }

        return RedirectToAction(nameof(Employees));
    }
}