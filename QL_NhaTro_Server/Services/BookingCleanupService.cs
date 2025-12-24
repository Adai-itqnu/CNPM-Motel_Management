using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.Models;

namespace QL_NhaTro_Server.Services
{
    /// <summary>
    /// Background service that runs periodically to:
    /// 1. Auto-cancel unpaid bookings after 5 minutes
    /// 2. Cancel contracts/bookings if check-in date is missed
    /// </summary>
    public class BookingCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _paymentTimeout = TimeSpan.FromMinutes(5);

        public BookingCleanupService(IServiceProvider serviceProvider, ILogger<BookingCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingCleanupService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredBookings();
                    await CancelMissedCheckIns();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in BookingCleanupService");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        /// <summary>
        /// Cancel bookings that haven't been paid within 5 minutes of creation
        /// </summary>
        private async Task CleanupExpiredBookings()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MotelManagementDbContext>();

            var cutoffTime = DateTime.Now.Subtract(_paymentTimeout);

            var expiredBookings = await db.Bookings
                .Include(b => b.Room)
                .Where(b => b.Status == BookingStatus.Pending 
                         && b.DepositStatus == DepositStatus.Pending
                         && b.CreatedAt < cutoffTime)
                .ToListAsync();

            if (expiredBookings.Any())
            {
                _logger.LogInformation($"Found {expiredBookings.Count} expired unpaid bookings to cancel");

                foreach (var booking in expiredBookings)
                {
                    booking.Status = BookingStatus.Cancelled;
                    booking.AdminNote = "Tự động hủy do không thanh toán trong 5 phút";
                    booking.UpdatedAt = DateTime.Now;

                    // Release room if it was reserved
                    if (booking.Room != null && booking.Room.Status == RoomStatus.Reserved)
                    {
                        booking.Room.Status = RoomStatus.Available;
                        booking.Room.CurrentUserId = null;
                        booking.Room.UpdatedAt = DateTime.Now;
                    }

                    _logger.LogInformation($"Auto-cancelled booking {booking.Id}");
                }

                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Cancel bookings and contracts where check-in date has passed without check-in
        /// </summary>
        private async Task CancelMissedCheckIns()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MotelManagementDbContext>();

            var today = DateTime.Today;

            // Find approved bookings with Draft contracts where CheckInDate has passed
            var missedCheckIns = await db.Bookings
                .Include(b => b.Room)
                .Include(b => b.Contract)
                .Where(b => b.Status == BookingStatus.Approved 
                         && b.CheckInDate < today
                         && b.Contract != null 
                         && b.Contract.Status == ContractStatus.Draft)
                .ToListAsync();

            if (missedCheckIns.Any())
            {
                _logger.LogInformation($"Found {missedCheckIns.Count} missed check-ins to cancel");

                foreach (var booking in missedCheckIns)
                {
                    // Cancel booking
                    booking.Status = BookingStatus.Cancelled;
                    booking.AdminNote = "Tự động hủy do không nhận phòng đúng hẹn";
                    booking.UpdatedAt = DateTime.Now;

                    // Cancel contract
                    if (booking.Contract != null)
                    {
                        booking.Contract.Status = ContractStatus.Cancelled;
                        booking.Contract.UpdatedAt = DateTime.Now;
                    }

                    // Release room
                    if (booking.Room != null)
                    {
                        booking.Room.Status = RoomStatus.Available;
                        booking.Room.CurrentUserId = null;
                        booking.Room.CurrentContractId = null;
                        booking.Room.UpdatedAt = DateTime.Now;
                    }

                    _logger.LogInformation($"Cancelled missed check-in booking {booking.Id}");
                }

                await db.SaveChangesAsync();
            }
        }
    }
}
