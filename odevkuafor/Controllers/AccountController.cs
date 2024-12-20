using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using odevkuafor.Models;

namespace odevkuafor.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                // E-posta kontrolü
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == user.Email);

                if (existingUser != null)
                {
                    // Hata mesajı ekle
                    ModelState.AddModelError("Email", "Bu e-posta zaten kayıtlı.");
                    return View(user);
                }

                // Parolayı hashle
                user.Password = HashPassword(user.Password);

                // Kullanıcıyı veritabanına ekle
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Kullanıcı kaydından sonra oturum açtır
                await SignInUser(user.Email);

                // Randevu oluşturma sayfasına yönlendir
                return RedirectToAction("Create", "Appointment");
            }

            return View(user);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    // Şifreyi doğrula
                    if (VerifyPassword(model.Password, existingUser.Password))
                    {
                        await SignInUser(existingUser.Email);
                        return RedirectToAction("Create", "Appointment");
                    }
                }

                // Hata mesajı ekle
                ModelState.AddModelError("", "Geçersiz e-posta veya şifre.");
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Kullanıcıyı çıkış yap
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Anasayfaya yönlendir
            return RedirectToAction("Index", "Home");
        }

        // Kullanıcıyı oturum açtır
        private async Task SignInUser(string email)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        // Parolayı hashleme
        private string HashPassword(string password)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("SabitBirAnahtar123!")))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }

        // Şifre doğrulama
        private bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("SabitBirAnahtar123!")))
            {
                var inputHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
                return Convert.ToBase64String(inputHash) == hashedPassword;
            }
        }
    }
}

