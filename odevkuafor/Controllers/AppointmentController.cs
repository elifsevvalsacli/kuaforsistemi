using System.Security.Claims;
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


        // Create POST metodu
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Employees = await _context.Employees.ToListAsync();
                model.Services = await _context.Services.ToListAsync();
                return View(model);
            }

            // UserId al
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            // Local DateTime'ı oluştur ve UTC'ye çevir
            var localDateTime = DateTime.Parse($"{model.AppointmentDate.ToString("yyyy-MM-dd")} {model.AppointmentTime}");
            localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Local);
            var utcDateTime = localDateTime.ToUniversalTime();

            var appointment = new Appointment
            {
                CustomerName = model.CustomerName,
                AppointmentDate = utcDateTime,
                ServiceId = model.ServiceId,
                EmployeeId = model.EmployeeId,
                UserId = userId
            };


            // Çifte rezervasyonu kontrol et
            var isTimeSlotAvailable = await IsTimeSlotAvailable(
                appointment.AppointmentDate,
                appointment.EmployeeId,
                appointment.ServiceId);

            if (!isTimeSlotAvailable)
            {
                ModelState.AddModelError("", "Bu saat dilimi artık müsait değil. Lütfen başka bir saat seçin.");
                model.Employees = await _context.Employees.ToListAsync();
                model.Services = await _context.Services.ToListAsync();
                return View(model);
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        private async Task<bool> IsTimeSlotAvailable(DateTimeOffset appointmentTime, int employeeId, int serviceId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return false;

            var appointmentEnd = appointmentTime.AddMinutes(service.DurationInMinutes);

            return !await _context.Appointments
                .AnyAsync(a => a.EmployeeId == employeeId &&
                              a.AppointmentDate < appointmentEnd &&
                              appointmentTime < a.AppointmentDate.AddMinutes(
                                  a.Service != null ? a.Service.DurationInMinutes : 0));
        }

        [HttpGet("GetAvailableHours")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableHours(DateTime date, int employeeId, int serviceId)
        {
            try
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service == null) return BadRequest("Hizmet bulunamadı");

                // Debug: Seçilen tarih ve servis bilgileri
                Console.WriteLine($"Selected Date: {date}, Service Duration: {service.DurationInMinutes} minutes");

                var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Local).ToUniversalTime();

                // Mevcut randevuları al
                var existingAppointments = await _context.Appointments
                    .Include(a => a.Service)
                    .Where(a => a.EmployeeId == employeeId &&
                               a.AppointmentDate.Date == utcDate.Date)
                    .ToListAsync();

                // Debug: Mevcut randevuları listele
                foreach (var app in existingAppointments)
                {
                    Console.WriteLine($"Existing appointment: {app.AppointmentDate.LocalDateTime:HH:mm}, Duration: {app.Service.DurationInMinutes}");
                }

                var availableSlots = new List<string>();
                var busySlots = new HashSet<DateTime>();

                // Tüm dolu slotları hesapla
                foreach (var appointment in existingAppointments)
                {
                    var appointmentStart = appointment.AppointmentDate.LocalDateTime;
                    var appointmentEnd = appointmentStart.AddMinutes(appointment.Service.DurationInMinutes);

                    // Her 30 dakikalık slotu işaretle
                    for (var time = appointmentStart; time < appointmentEnd; time = time.AddMinutes(30))
                    {
                        busySlots.Add(time);
                        Console.WriteLine($"Marked as busy: {time:HH:mm}");
                    }
                }

                // Tüm olası slotları kontrol et
                var workDayStart = date.Date.AddHours(9); // 09:00
                var workDayEnd = date.Date.AddHours(18);  // 18:00

                for (var time = workDayStart; time < workDayEnd; time = time.AddMinutes(30))
                {
                    var isAvailable = true;

                    // Hizmet süresi boyunca her 30 dakikalık slotu kontrol et
                    for (var checkTime = time;
                         checkTime < time.AddMinutes(service.DurationInMinutes);
                         checkTime = checkTime.AddMinutes(30))
                    {
                        if (busySlots.Contains(checkTime))
                        {
                            isAvailable = false;
                            Console.WriteLine($"Slot {time:HH:mm} is not available due to conflict at {checkTime:HH:mm}");
                            break;
                        }
                    }

                    // Geçmiş saat kontrolü
                    if (date.Date == DateTime.Today && time <= DateTime.Now)
                    {
                        isAvailable = false;
                        Console.WriteLine($"Slot {time:HH:mm} is in the past");
                    }

                    // Hizmet süresi mesai saatini aşıyor mu kontrolü
                    if (time.AddMinutes(service.DurationInMinutes) > workDayEnd)
                    {
                        isAvailable = false;
                        Console.WriteLine($"Slot {time:HH:mm} extends beyond working hours");
                    }

                    if (isAvailable)
                    {
                        availableSlots.Add(time.ToString("HH:mm"));
                        Console.WriteLine($"Added available slot: {time:HH:mm}");
                    }
                }

                return Json(availableSlots);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAvailableHours: {ex}");
                return BadRequest($"Bir hata oluştu: {ex.Message}");
            }
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
                try
                {
                    var isAvailable = await IsTimeSlotAvailable(
                        appointment.AppointmentDate,
                        appointment.EmployeeId,
                        appointment.ServiceId);

                    if (!isAvailable)
                    {
                        ModelState.AddModelError("", "Seçilen saat dilimi dolu");
                        await LoadDropdownData();
                        return View(appointment);
                    }

                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Appointments.AnyAsync(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
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

        private async Task LoadDropdownData()
        {
            ViewBag.Services = await _context.Services.ToListAsync();
            ViewBag.Employees = await _context.Employees.ToListAsync();
        }
    }
}
