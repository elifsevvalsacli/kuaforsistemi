using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using odevkuafor.Models;

namespace odevkuafor.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor
        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Randevu Listesi
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Service)  // Hizmeti dahil et
                .Include(a => a.Employee) // Çalışanı dahil et
                .ToListAsync();

            return View(appointments);
        }

        // Randevu Oluşturma Sayfası (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Hizmetler ve çalışanlar verisini ViewBag ile gönder
            ViewBag.Services = await _context.Services.ToListAsync();
            ViewBag.Employees = await _context.Employees.ToListAsync();

            return View();
        }

        // Randevu Oluşturma İşlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                // Çalışanın uygunluğunu kontrol et
                bool isEmployeeAvailable = await _context.Appointments
                    .AnyAsync(a =>
                        a.EmployeeId == appointment.EmployeeId &&
                        a.AppointmentDate == appointment.AppointmentDate);

                if (isEmployeeAvailable)
                {
                    ModelState.AddModelError("", "Bu çalışan bu saatte başka bir randevuya sahip.");

                    // Hizmetler ve çalışanlar verisini ViewBag ile tekrar gönder
                    ViewBag.Services = await _context.Services.ToListAsync();
                    ViewBag.Employees = await _context.Employees.ToListAsync();
                    return View(appointment);
                }

                // Randevuyu kaydet
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index)); // Randevu listesine yönlendir
            }

            // Model geçersizse, hizmetler ve çalışanlar verisini ViewBag ile tekrar gönder
            ViewBag.Services = await _context.Services.ToListAsync();
            ViewBag.Employees = await _context.Employees.ToListAsync();
            return View(appointment);
        }

        // Randevu Düzenleme Sayfası (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Hizmetler ve çalışanlar verisini ViewBag ile gönder
            ViewBag.Services = await _context.Services.ToListAsync();
            ViewBag.Employees = await _context.Employees.ToListAsync();

            return View(appointment);
        }

        // Randevu Düzenleme İşlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                // Çalışanın uygunluğunu kontrol et
                bool isEmployeeAvailable = await _context.Appointments
                    .AnyAsync(a =>
                        a.EmployeeId == appointment.EmployeeId &&
                        a.AppointmentDate == appointment.AppointmentDate &&
                        a.Id != appointment.Id); // Kendisi hariç

                if (isEmployeeAvailable)
                {
                    ModelState.AddModelError("", "Bu çalışan bu saatte başka bir randevuya sahip.");

                    // Hizmetler ve çalışanlar verisini ViewBag ile tekrar gönder
                    ViewBag.Services = await _context.Services.ToListAsync();
                    ViewBag.Employees = await _context.Employees.ToListAsync();
                    return View(appointment);
                }

                // Randevuyu güncelle
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index)); // Randevu listesine yönlendir
            }

            // Model geçersizse, hizmetler ve çalışanlar verisini ViewBag ile tekrar gönder
            ViewBag.Services = await _context.Services.ToListAsync();
            ViewBag.Employees = await _context.Employees.ToListAsync();
            return View(appointment);
        }

        // Randevu Silme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Randevu listesine yönlendir
        }
    }
}
