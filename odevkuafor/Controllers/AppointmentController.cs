// AppointmentController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using odevkuafor.Models;

namespace odevkuafor.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .ToListAsync();

            return View(appointments);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new AppointmentViewModel
            {
                Employees = await _context.Employees.ToListAsync(),
                Services = await _context.Services.ToListAsync(),
                AppointmentDate = DateTime.Now
            };

            return View(viewModel);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (!TimeSpan.TryParse(viewModel.AppointmentTime, out TimeSpan appointmentTime))
                    {
                        ModelState.AddModelError("AppointmentTime", "Geçersiz saat formatı");
                        return View(viewModel);
                    }

                    var combinedDateTime = viewModel.AppointmentDate.Date + appointmentTime;
                    var localTimeZone = TimeZoneInfo.Local;
                    var appointmentDateOffset = new DateTimeOffset(combinedDateTime, localTimeZone.GetUtcOffset(combinedDateTime));

                    var appointment = new Appointment
                    {
                        CustomerName = viewModel.CustomerName,
                        AppointmentDate = appointmentDateOffset,
                        EmployeeId = viewModel.EmployeeId,
                        ServiceId = viewModel.ServiceId
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Randevu oluşturulurken bir hata oluştu: " + ex.Message);
                }
            }

            viewModel.Employees = await _context.Employees.ToListAsync();
            viewModel.Services = await _context.Services.ToListAsync();
            return View(viewModel);
        }

        [HttpGet("Edit/{id}")]
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

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

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

        [HttpPost("Delete/{id}")]
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

        [HttpGet("GetAvailableHours")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableHours([FromQuery] string date, [FromQuery] int employeeId, [FromQuery] int serviceId)
        {
            if (string.IsNullOrEmpty(date) || employeeId <= 0 || serviceId <= 0)
            {
                return BadRequest("Geçersiz parametreler");
            }

            try
            {
                // Temel kontroller
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                {
                    return BadRequest("Geçersiz tarih formatı");
                }

                var service = await _context.Services.FindAsync(serviceId);
                if (service == null)
                {
                    return BadRequest("Hizmet bulunamadı");
                }

                // Çalışanın bu hizmeti verip vermediğini kontrol et
                var employeeService = await _context.EmployeeServices
                    .AnyAsync(es => es.EmployeeId == employeeId && es.ServiceId == serviceId);
                if (!employeeService)
                {
                    return BadRequest("Çalışan bu hizmeti vermiyor");
                }

                var businessHours = new List<string>();
                var startHour = 9;  // İş başlangıç saati
                var endHour = 19;   // İş bitiş saati

                // Mevcut randevuları getir
                var existingAppointments = await _context.Appointments
                    .Where(a => a.EmployeeId == employeeId &&
                           a.AppointmentDate.Date == selectedDate.Date)
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync();

                // Her yarım saatlik dilim için kontrol
                for (int hour = startHour; hour < endHour; hour++)
                {
                    for (int minute = 0; minute < 60; minute += 30)
                    {
                        var currentTime = selectedDate.Date.AddHours(hour).AddMinutes(minute);

                        // Hizmet süresini ekleyerek bitiş zamanını hesapla
                        var endTime = currentTime.AddMinutes(service.DurationInMinutes);

                        // Mesai saatleri dışına taşıyorsa bu saati atla
                        if (endTime.Hour >= endHour || (endTime.Hour == endHour && endTime.Minute > 0))
                        {
                            continue;
                        }

                        bool isAvailable = true;

                        // Mevcut randevularla çakışma kontrolü
                        foreach (var existing in existingAppointments)
                        {
                            var existingEndTime = existing.AppointmentDate.DateTime.AddMinutes(service.DurationInMinutes);

                            // Çakışma kontrolü
                            if ((currentTime >= existing.AppointmentDate.DateTime && currentTime < existingEndTime) ||
                                (endTime > existing.AppointmentDate.DateTime && endTime <= existingEndTime))
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
                // Hata durumunda log tutuyoruz
                Console.WriteLine($"GetAvailableHours Error: {ex.Message}");
                return StatusCode(500, "Bir hata oluştu");
            }
        }
        private async Task LoadDropdownData()
        {
            ViewBag.Services = await _context.Services.ToListAsync() ?? new List<Service>();
            ViewBag.Employees = await _context.Employees.ToListAsync() ?? new List<Employee>();
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

            return conflictingAppointment == null;
        }
    }
}
