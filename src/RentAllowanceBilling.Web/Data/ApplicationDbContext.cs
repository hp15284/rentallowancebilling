using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentAllowanceBilling.Web.Models;

namespace RentAllowanceBilling.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<RentAllowanceBill> RentAllowanceBills => Set<RentAllowanceBill>();
    public DbSet<RentAllowanceBillTrip> RentAllowanceBillTrips => Set<RentAllowanceBillTrip>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Branch>(entity =>
        {
            entity.HasIndex(b => b.Code).IsUnique();
        });

        builder.Entity<Employee>(entity =>
        {
            entity.HasOne(e => e.Branch)
                .WithMany(b => b.Employees)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ApplicationUser)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.ApplicationUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Branch)
                .WithMany()
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<RentAllowanceBill>(entity =>
        {
            entity.HasOne(b => b.Employee)
                .WithMany(e => e.Bills)
                .HasForeignKey(b => b.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Branch)
                .WithMany()
                .HasForeignKey(b => b.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(b => b.BillNumber);
        });

        builder.Entity<RentAllowanceBillTrip>(entity =>
        {
            entity.HasOne(t => t.RentAllowanceBill)
                .WithMany(b => b.Trips)
                .HasForeignKey(t => t.RentAllowanceBillId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
