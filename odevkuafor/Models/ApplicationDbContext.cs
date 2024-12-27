using Microsoft.EntityFrameworkCore;
using odevkuafor.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<EmployeeService> EmployeeServices { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Service entity configuration
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Price).IsRequired();

            // Service silindiğinde ilişkili EmployeeServices kayıtları da silinsin
            entity.HasMany(s => s.EmployeeServices)
                  .WithOne(es => es.Service)
                  .HasForeignKey(es => es.ServiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Appointment entity configuration
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.Id);

            // Service-Appointment ilişkisi
            entity.HasOne(a => a.Service)
                  .WithMany(s => s.Appointments)
                  .HasForeignKey(a => a.ServiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // EmployeeService configuration
        modelBuilder.Entity<EmployeeService>(entity =>
        {
            entity.HasKey(es => new { es.EmployeeId, es.ServiceId });
        });
    }

    // Admin kullanıcısını veritabanına ekleyecek Seed metodu
    public static void SeedAdminUser(ApplicationDbContext context)
    {
        var adminEmail = "b211210068@sakarya.edu.tr";
        var adminPassword = "sau";

        // Admin kullanıcısını kontrol et
        var adminUser = context.Users.FirstOrDefault(u => u.Email == adminEmail);

        if (adminUser == null)
        {
            // Yeni hash oluştur
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(adminPassword);

            adminUser = new User
            {
                Email = adminEmail,
                Password = hashedPassword,
                IsAdmin = true
            };

            context.Users.Add(adminUser);
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yapabilirsiniz
                Console.WriteLine($"Admin user seed error: {ex.Message}");
            }
        }
    }
}