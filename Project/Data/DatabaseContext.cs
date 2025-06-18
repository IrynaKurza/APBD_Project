using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Data;

public class DatabaseContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeRole> EmployeeRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<IndividualClient> IndividualClients { get; set; }
    public DbSet<CompanyClient> CompanyClients { get; set; }
    public DbSet<Software> Software { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Discount> Discounts { get; set; }

    protected DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Client inheritance
        modelBuilder.Entity<Client>(c =>
        {
            c.ToTable("Client");
            c.HasDiscriminator<string>("ClientType")
                .HasValue<IndividualClient>("Individual")
                .HasValue<CompanyClient>("Company");
        });

        // Configure Individual Client
        modelBuilder.Entity<IndividualClient>(i =>
        {
            i.HasIndex(e => e.PESEL).IsUnique();
        });

        // Configure Company Client
        modelBuilder.Entity<CompanyClient>(c =>
        {
            c.HasIndex(e => e.KRSNumber).IsUnique();
        });

        // Seed employee roles
        modelBuilder.Entity<EmployeeRole>().HasData(
            new EmployeeRole { Id = 1, Name = "standard" },
            new EmployeeRole { Id = 2, Name = "admin" }
        );

        // Seed software
        modelBuilder.Entity<Software>().HasData(new List<Software>()
        {
            new Software() 
            { 
                Id = 1, 
                Name = "AccountingPro", 
                Description = "Professional accounting software",
                CurrentVersion = "2.1.0",
                Category = "finances",
                AnnualLicenseCost = 2000m 
            },
            new Software() 
            { 
                Id = 2, 
                Name = "EduManager", 
                Description = "Education management system",
                CurrentVersion = "1.5.2",
                Category = "education",
                AnnualLicenseCost = 1500m 
            }
        });

        // Seed discounts
        modelBuilder.Entity<Discount>().HasData(new List<Discount>()
        {
            new Discount() 
            { 
                Id = 1, 
                Name = "Black Friday", 
                Percentage = 10m,
                StartDate = new DateTime(2025, 11, 25),
                EndDate = new DateTime(2025, 11, 30),
                IsForContracts = true,
                SoftwareId = null
            }
        });
    }
}