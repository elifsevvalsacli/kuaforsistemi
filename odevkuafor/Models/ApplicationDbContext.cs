// ApplicationDbContext.cs
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

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Price).IsRequired();
            entity.HasMany(s => s.EmployeeServices)
                  .WithOne(es => es.Service)
                  .HasForeignKey(es => es.ServiceId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.Service)
                  .WithMany(s => s.Appointments)
                  .HasForeignKey(a => a.ServiceId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmployeeService>(entity =>
        {
            entity.HasKey(es => new { es.EmployeeId, es.ServiceId });
        });
    }

    public static async Task SeedAdminUser(ApplicationDbContext context)
    {
        if (!context.Users.Any(u => u.Email == "b211210068@sakarya.edu.tr"))
        {
            var adminUser = new User
            {
                Email = "b211210068@sakarya.edu.tr",
                Password = BCrypt.Net.BCrypt.HashPassword("sau"),
                IsAdmin = true
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }
    }
}
