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
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.IdCard).IsUnique();
                entity.HasIndex(e => e.Role); // Index for role filtering
                
                entity.Property(e => e.Role)
                    .HasConversion<string>();
            });

            // Room entity configuration
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique(); // Keep existing unique index
                entity.HasIndex(e => e.Status); // Index for status filtering
                entity.HasIndex(e => e.Floor); // Index for floor filtering
                entity.HasIndex(e => e.Price); // Index for price filtering
                
                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.CurrentUser)
                    .WithMany()
                    .HasForeignKey(e => e.CurrentUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // RoomAmenity configuration
            modelBuilder.Entity<RoomAmenity>(entity =>
            {
                entity.HasIndex(e => e.RoomId); // Index for room lookups
                
                entity.HasOne(e => e.Room)
                    .WithMany(r => r.Amenities)
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RoomImage configuration
            modelBuilder.Entity<RoomImage>(entity =>
            {
                entity.HasIndex(e => e.RoomId); // Index for room lookups
                entity.HasIndex(e => e.IsPrimary); // Index for primary image queries
                
                entity.HasOne(e => e.Room)
                    .WithMany(r => r.Images)
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
                entity.HasIndex(e => e.Status); // Index for status filtering
                entity.HasIndex(e => e.UserId); // Index for user bookings
                entity.HasIndex(e => e.CreatedAt); // Index for sorting by date

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
                entity.Property(e => e.Status) // Original property name
                    .HasConversion<string>();
                entity.HasIndex(e => e.UserId); // Index for user contracts
                entity.HasIndex(e => e.RoomId); // Index for room contracts
                entity.HasIndex(e => e.StartDate); // Index for date queries
                entity.HasIndex(e => e.EndDate); // Index for expiry checks

                entity.HasOne(e => e.Room)
                    .WithMany(e => e.Contracts)
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Bill entity configuration
            modelBuilder.Entity<Bill>(entity =>
            {
                entity.Property(e => e.Status) // Original property name
                    .HasConversion<string>();
                entity.HasIndex(e => e.UserId); // Index for user bills
                entity.HasIndex(e => new { e.Month, e.Year }); // Composite index for monthly queries
                entity.HasIndex(e => e.Status); // Index for payment status filtering (using original 'Status' property)

                entity.HasOne(e => e.Contract)
                    .WithMany(e => e.Bills)
                    .HasForeignKey(e => e.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Room)
                    .WithMany(e => e.Bills)
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
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
                entity.HasIndex(e => e.UserId); // Index for user payments
                entity.HasIndex(e => e.Status); // Index for status filtering
                entity.HasIndex(e => e.PaymentDate); // Index for date queries
                entity.HasIndex(e => new { e.PaymentDate, e.Status }); // Composite for stats queries

                entity.HasOne(e => e.Bill)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.BillId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Booking)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification entity configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.Type)
                    .HasConversion<string>();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.UserId, e.IsRead }); // Composite for unread count

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Sender)
                    .WithMany()
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
