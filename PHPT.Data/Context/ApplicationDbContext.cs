using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PHPT.Data.Entities;

namespace PHPT.Data.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Facility> Facilities { get; set; }
    public DbSet<Clinic> Clinics { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<PatientWallet> PatientWallets { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure global query filter for soft delete
        modelBuilder.Entity<Facility>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Clinic>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Patient>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PatientWallet>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WalletTransaction>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Appointment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<InvoiceItem>().HasQueryFilter(e => !e.IsDeleted);

        // Facility relationships
        modelBuilder.Entity<Facility>()
            .HasMany(f => f.Clinics)
            .WithOne(c => c.Facility)
            .HasForeignKey(c => c.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Facility>()
            .HasMany(f => f.Patients)
            .WithOne(p => p.Facility)
            .HasForeignKey(p => p.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Patient - Wallet relationship (One-to-One)
        modelBuilder.Entity<Patient>()
            .HasOne(p => p.Wallet)
            .WithOne(w => w.Patient)
            .HasForeignKey<PatientWallet>(w => w.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Wallet - Transactions relationship
        modelBuilder.Entity<PatientWallet>()
            .HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        // Appointment relationships
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Clinic)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        // Invoice relationships
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Appointment)
            .WithOne(a => a.Invoice)
            .HasForeignKey<Invoice>(i => i.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Items)
            .WithOne(ii => ii.Invoice)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Transaction - Invoice relationship
        modelBuilder.Entity<WalletTransaction>()
            .HasOne(t => t.Invoice)
            .WithMany(i => i.Transactions)
            .HasForeignKey(t => t.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // ApplicationUser relationships
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Facility)
            .WithMany()
            .HasForeignKey(u => u.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Creator)
            .WithMany()
            .HasForeignKey(u => u.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Decimal precision configuration
        modelBuilder.Entity<PatientWallet>()
            .Property(w => w.Balance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<WalletTransaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<WalletTransaction>()
            .Property(t => t.BalanceBefore)
            .HasPrecision(18, 2);

        modelBuilder.Entity<WalletTransaction>()
            .Property(t => t.BalanceAfter)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.DiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.DiscountPercentage)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<InvoiceItem>()
            .Property(ii => ii.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<InvoiceItem>()
            .Property(ii => ii.TotalPrice)
            .HasPrecision(18, 2);

        // Indexes
        modelBuilder.Entity<Patient>()
            .HasIndex(p => p.PatientCode)
            .IsUnique();

        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.ClinicId, a.AppointmentDate, a.AppointmentTime });

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
