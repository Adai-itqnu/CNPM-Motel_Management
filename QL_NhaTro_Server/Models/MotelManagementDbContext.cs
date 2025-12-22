using Microsoft.EntityFrameworkCore;

namespace QL_NhaTro_Server.Models
{
    public class MotelManagementDbContext : DbContext
    {
        public MotelManagementDbContext(DbContextOptions<MotelManagementDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomAmenity> RoomAmenities { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.IdCard).IsUnique();
                
                entity.Property(e => e.Role)
                    .HasConversion<string>();
            });

            // Room entity configuration
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                
                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.CurrentTenant)
                    .WithMany()
                    .HasForeignKey(e => e.CurrentTenantId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(e => e.Amenities)
                    .WithOne(e => e.Room)
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Images)
                    .WithOne(e => e.Room)
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Booking entity configuration
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.Property(e => e.DepositStatus)
                    .HasConversion<string>();

                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.Room)
                    .WithMany(e => e.Bookings)
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Bookings)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Contract entity configuration
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.Room)
                    .WithMany(e => e.Contracts)
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Tenant)
                    .WithMany(e => e.Contracts)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Bill entity configuration
            modelBuilder.Entity<Bill>(entity =>
            {
                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.Contract)
                    .WithMany(e => e.Bills)
                    .HasForeignKey(e => e.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Room)
                    .WithMany(e => e.Bills)
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Tenant)
                    .WithMany(e => e.Bills)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Composite index for month/year per contract
                entity.HasIndex(e => new { e.ContractId, e.Month, e.Year }).IsUnique();
            });

            // Payment entity configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(e => e.PaymentType)
                    .HasConversion<string>();

                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.Bill)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.BillId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Booking)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Tenant)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
