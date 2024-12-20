using Microsoft.EntityFrameworkCore;
using odevkuafor.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Servisleri ekliyoruz
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); // PostgreSQL için UseNpgsql

// MVC desteğini ekliyoruz
builder.Services.AddControllersWithViews();

// Kimlik doğrulama ve yetkilendirme servislerini ekliyoruz
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giriş yapma sayfası
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Cookie süresi
    });

builder.Services.AddAuthorization(options =>
{
    // Özel yetkilendirme politikaları ekleyebilirsiniz
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Oturum desteği
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum süresi
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Gerekli oturum cookie
});

var app = builder.Build();

// HTTP request pipeline yapılandırması
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();  // Geliştirme ortamında hata sayfası
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication ve Authorization middleware'lerini ekliyoruz
app.UseSession(); // Oturum yönetimi
app.UseAuthentication(); // Kimlik doğrulama işlemi
app.UseAuthorization();  // Yetkilendirme işlemi

// Veritabanı seed işlemi
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    ApplicationDbContext.SeedAdminUser(dbContext);  // Admin kullanıcısını ekle
}

// Routing işlemi
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Özel bir yönlendirme (isteğe bağlı)
app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Login}/{id?}",
    defaults: new { controller = "Account" });

// Admin için özel bir yönlendirme
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

app.Run();
