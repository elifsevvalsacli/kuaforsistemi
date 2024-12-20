using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using odevkuafor.Models;

namespace odevkuafor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Giriş Sayfası (GET)
        public IActionResult Index()
        {
            return View();
        }

        // Giriş İşlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Kullanıcıyı veritabanında arama
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // Admin kontrolü
                if (user.IsAdmin)
                {
                    // Admin giriş başarılı, admin paneline yönlendir
                    return RedirectToAction("Index", "Admin");  // Admin Index sayfasına yönlendir
                }

                // Normal kullanıcı giriş başarılı, giriş sayfasına geri gönder
                return RedirectToAction(nameof(Index));  // Normal kullanıcıyı giriş sayfasına yönlendir
            }

            // Hatalı giriş, hata mesajı göster
            ModelState.AddModelError("", "Geçersiz e-posta veya şifre.");
            return View(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

