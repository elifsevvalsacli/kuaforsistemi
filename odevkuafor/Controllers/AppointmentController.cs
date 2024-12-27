using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using odevkuafor.Models;

namespace odevkuafor.Controllers
{
    [Authorize]
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Tarih ve saati birleştir
                var appointmentTime = TimeSpan.Parse(viewModel.AppointmentTime);
                var appointmentDate = viewModel.AppointmentDate.Date.Add(appointmentTime);

                var appointment = new Appointment
                {
                    CustomerName = viewModel.CustomerName,
                    AppointmentDate = appointmentDate,
                    EmployeeId = viewModel.EmployeeId,
                    ServiceId = viewModel.ServiceId
                };

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
                var service = await _context.Services.FindAsync(appointment.ServiceId);
                if (service == null) return false;

                var appointmentDate = appointment.AppointmentDate;
                var startTime = appointmentDate;
                var endTime = startTime.AddMinutes(service.DurationInMinutes);

                var conflictingAppointment = await _context.Appointments
                    .Where(a =>
                        a.EmployeeId == appointment.EmployeeId &&
                        a.Id != (excludeAppointmentId ?? 0) &&
                        ((a.AppointmentDate <= startTime && a.AppointmentDate.AddMinutes(service.DurationInMinutes) > startTime) ||
                         (a.AppointmentDate < endTime && a.AppointmentDate.AddMinutes(service.DurationInMinutes) >= endTime)))
                    .FirstOrDefaultAsync();

                return conflictingAppointment == null; // Return true if NO conflicts found
            }

            [HttpGet]
            public async Task<JsonResult> GetAvailableHours([FromQuery] string date, [FromQuery] int employeeId, [FromQuery] int serviceId)
            {
                try
                {
                    var selectedDate = DateTime.Parse(date);
                    var businessHours = new List<string>();
                    var startHour = 9;  // 09:00
                    var endHour = 19;   // 19:00

                    // Get selected service
                    var service = await _context.Services.FindAsync(serviceId);
                    if (service == null) return Json(new string[] { });

                    // Get existing appointments for the day
                    var existingAppointments = await _context.Appointments
                        .Where(a => a.AppointmentDate.Date == selectedDate.Date &&
                                   a.EmployeeId == employeeId)
                        .OrderBy(a => a.AppointmentDate)
                        .ToListAsync();

                    // Check all time slots
                    for (int hour = startHour; hour < endHour; hour++)
                    {
                        for (int minute = 0; minute < 60; minute += 30)
                        {
                            var currentSlot = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hour, minute, 0);
                            var endSlot = currentSlot.AddMinutes(service.DurationInMinutes);

                            // Check if slot ends within business hours
                            if (endSlot.Hour >= endHour) continue;

                            // Check for conflicts
                            bool isAvailable = true;
                            foreach (var appointment in existingAppointments)
                            {
                                var appointmentEnd = appointment.AppointmentDate.AddMinutes(service.DurationInMinutes);

                                if ((currentSlot >= appointment.AppointmentDate && currentSlot < appointmentEnd) ||
                                    (endSlot > appointment.AppointmentDate && endSlot <= appointmentEnd) ||
                                    (currentSlot <= appointment.AppointmentDate && endSlot >= appointmentEnd))
                                {
                                    isAvailable = false;
                                    break;
                                }
                            }

                            if (isAvailable)
                            {
                                businessHours.Add($"{hour:D2}:{minute:D2}");
                            }
                        }
                    }

                    return Json(businessHours);
                }
                catch (Exception ex)
                {
                    // Log the exception
                    return Json(new string[] { });
                }
            }
        
        // Dropdown Verilerini Yükleme
        private async Task LoadDropdownData()
        {
            ViewBag.Services = await _context.Services.ToListAsync() ?? new List<Service>();
            ViewBag.Employees = await _context.Employees.ToListAsync() ?? new List<Employee>();
        }
    }
}
