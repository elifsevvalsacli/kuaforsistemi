using Microsoft.EntityFrameworkCore;
using odevkuafor.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL bağlantısı için DbContext yapılandırması
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); // PostgreSQL bağlantısı

// MVC desteği
builder.Services.AddControllersWithViews();

// Kimlik doğrulama ve yetkilendirme servisleri
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giriş sayfası
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetki reddedildi sayfası
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Çerez geçerlilik süresi
        options.SlidingExpiration = true; // Çerez süresi her istekle uzatılır
    });

builder.Services.AddAuthorization(options =>
{
    // Admin kullanıcılar için özel yetkilendirme politikası
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Oturum desteği yapılandırması
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum zaman aşımı süresi
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Gerekli çerez işaretleme
});

var app = builder.Build();

// Hata işleme ve güvenlik yapılandırması
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTPS Strict Transport Security
}
else
{
    app.UseDeveloperExceptionPage(); // Geliştirme ortamı hata sayfası
}

// HTTPS yönlendirme ve statik dosyalar
app.UseHttpsRedirection();
app.UseStaticFiles();

// Routing ve Middleware yapılandırması
app.UseRouting();
app.UseSession(); // Oturum yönetimi
app.UseAuthentication(); // Kimlik doğrulama
app.UseAuthorization();  // Yetkilendirme

// Veritabanı seed işlemi (Admin kullanıcısı ekleme)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    ApplicationDbContext.SeedAdminUser(dbContext); // Admin kullanıcıyı ekle
}

// Varsayılan rotalar
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Hesap yönetimi rotaları
app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Login}/{id?}",
    defaults: new { controller = "Account" });

// Admin yönetim rotaları
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

app.Run();
