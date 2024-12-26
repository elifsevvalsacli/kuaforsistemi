using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using odevkuafor.Models;

namespace odevkuafor.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Randevu Listesi
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .ToListAsync();

            return View(appointments);
        }

        // Randevu Oluşturma Sayfası (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new AppointmentViewModel
            {
                Employees = await _context.Employees.ToListAsync(),
                Services = await _context.Services.ToListAsync(),
                AppointmentDate = DateTime.Now // Varsayılan değer
            };

            return View(viewModel);
        }
        // Randevu Oluşturma İşlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var appointment = new Appointment
                {
                    CustomerName = viewModel.CustomerName,
                    AppointmentDate = viewModel.AppointmentDate,
                    EmployeeId = viewModel.EmployeeId,
                    ServiceId = viewModel.ServiceId
                };

                if (await IsEmployeeAvailable(appointment))
                {
                    ModelState.AddModelError("", "Bu çalışan bu saatte başka bir randevuya sahip.");
                    viewModel.Employees = await _context.Employees.ToListAsync();
                    viewModel.Services = await _context.Services.ToListAsync();
                    return View(viewModel);
                }

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.Employees = await _context.Employees.ToListAsync();
            viewModel.Services = await _context.Services.ToListAsync();
            return View(viewModel);
        }

        // Randevu Düzenleme Sayfası (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            await LoadDropdownData();
            return View(appointment);
        }

        // Randevu Düzenleme İşlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                if (await IsEmployeeAvailable(appointment, appointment.Id))
                {
                    ModelState.AddModelError("", "Bu çalışan bu saatte başka bir randevuya sahip.");
                    await LoadDropdownData();
                    return View(appointment);
                }

                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData();
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

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> IsEmployeeAvailable(Appointment appointment, int? excludeAppointmentId = null)
        {
            var appointmentDate = appointment.AppointmentDate;
            var startTime = appointmentDate.Date.Add(appointmentDate.TimeOfDay);
            var endTime = startTime.AddHours(1); // Varsayılan olarak 1 saatlik randevu

            return await _context.Appointments
                .AnyAsync(a =>
                    a.EmployeeId == appointment.EmployeeId &&
                    a.AppointmentDate >= startTime &&
                    a.AppointmentDate < endTime &&
                    (excludeAppointmentId == null || a.Id != excludeAppointmentId));
        }
        // Dropdown Verilerini Yükleme
        private async Task LoadDropdownData()
        {
            ViewBag.Services = await _context.Services.ToListAsync() ?? new List<Service>();
            ViewBag.Employees = await _context.Employees.ToListAsync() ?? new List<Employee>();
        }
    }
}
