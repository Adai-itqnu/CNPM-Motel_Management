using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.DTOs;
using QL_NhaTro_Server.Models;

namespace QL_NhaTro_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly MotelManagementDbContext _db;

        public StatisticsController(MotelManagementDbContext db)
        {
            _db = db;
        }

        // GET /api/statistics/summary 
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                // SINGLE QUERY for room stats
                var roomStats = await _db.Rooms
                    .GroupBy(r => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Occupied = g.Count(r => r.Status == RoomStatus.Occupied),
                        Available = g.Count(r => r.Status == RoomStatus.Available)
                    })
                    .FirstOrDefaultAsync();

                // SINGLE QUERY for payment stats  
                var monthlyRevenue = await _db.Payments
                    .Where(p => p.PaymentDate.HasValue 
                             && p.PaymentDate.Value.Month == currentMonth 
                             && p.PaymentDate.Value.Year == currentYear
                             && p.Status == PaymentStatus.Success)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                // SINGLE QUERY for pending bookings
                var pendingBookings = await _db.Bookings
                    .CountAsync(b => b.Status == BookingStatus.Pending);

                var stats = new DashboardStatsDto
                {
                    TotalRooms = roomStats?.Total ?? 0,
                    OccupiedRooms = roomStats?.Occupied ?? 0,
                    AvailableRooms = roomStats?.Available ?? 0,
                    MonthlyRevenue = monthlyRevenue,
                    PendingBookings = pendingBookings
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thống kê", error = ex.Message });
            }
        }

        // GET /api/statistics/room-status - OPTIMIZED: 1 query thay vì 4
        [HttpGet("room-status")]
        public async Task<IActionResult> GetRoomStatus()
        {
            try
            {
                var stats = await _db.Rooms
                    .GroupBy(r => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Occupied = g.Count(r => r.Status == RoomStatus.Occupied),
                        Available = g.Count(r => r.Status == RoomStatus.Available),
                        Maintenance = g.Count(r => r.Status == RoomStatus.Maintenance)
                    })
                    .FirstOrDefaultAsync();

                var total = stats?.Total ?? 0;
                var status = new RoomStatusDto
                {
                    Total = total,
                    Occupied = total > 0 ? (int)Math.Round((double)(stats?.Occupied ?? 0) / total * 100) : 0,
                    Available = total > 0 ? (int)Math.Round((double)(stats?.Available ?? 0) / total * 100) : 0,
                    Maintenance = total > 0 ? (int)Math.Round((double)(stats?.Maintenance ?? 0) / total * 100) : 0
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy trạng thái phòng", error = ex.Message });
            }
        }

        // GET /api/statistics/room-details - Số lượng phòng thực tế cho biểu đồ
        [HttpGet("room-details")]
        public async Task<IActionResult> GetRoomDetails()
        {
            try
            {
                var stats = await _db.Rooms
                    .GroupBy(r => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Occupied = g.Count(r => r.Status == RoomStatus.Occupied),
                        Available = g.Count(r => r.Status == RoomStatus.Available),
                        Maintenance = g.Count(r => r.Status == RoomStatus.Maintenance),
                        Reserved = g.Count(r => r.Status == RoomStatus.Reserved)
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    total = stats?.Total ?? 0,
                    occupied = stats?.Occupied ?? 0,
                    available = stats?.Available ?? 0,
                    maintenance = stats?.Maintenance ?? 0,
                    reserved = stats?.Reserved ?? 0,
                    occupancyRate = stats?.Total > 0 
                        ? Math.Round((double)(stats?.Occupied ?? 0) / stats.Total * 100, 1) 
                        : 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy chi tiết phòng", error = ex.Message });
            }
        }

        // GET /api/statistics/revenue-chart - OPTIMIZED: 1 query với GROUP BY thay vì loop
        [HttpGet("revenue-chart")]
        public async Task<IActionResult> GetRevenueChart([FromQuery] int months = 6)
        {
            try
            {
                var startDate = DateTime.Now.AddMonths(-months + 1).Date;
                
                // SINGLE QUERY với GROUP BY
                var revenueData = await _db.Payments
                    .Where(p => p.PaymentDate.HasValue
                             && p.PaymentDate.Value >= startDate
                             && p.Status == PaymentStatus.Success)
                    .GroupBy(p => new { p.PaymentDate!.Value.Year, p.PaymentDate.Value.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(p => p.Amount)
                    })
                    .ToListAsync();

                // Fill in missing months with 0
                var monthlyData = new List<MonthlyRevenueDto>();
                for (int i = months - 1; i >= 0; i--)
                {
                    var targetDate = DateTime.Now.AddMonths(-i);
                    var revenue = revenueData
                        .FirstOrDefault(r => r.Year == targetDate.Year && r.Month == targetDate.Month)
                        ?.Revenue ?? 0;

                    monthlyData.Add(new MonthlyRevenueDto
                    {
                        Month = targetDate.Month,
                        MonthName = $"T{targetDate.Month}",
                        Revenue = revenue
                    });
                }

                return Ok(new RevenueChartDto { MonthlyData = monthlyData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy biểu đồ doanh thu", error = ex.Message });
            }
        }

        // GET /api/statistics/recent-activities - OPTIMIZED: Projection sớm
        [HttpGet("recent-activities")]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int limit = 5)
        {
            try
            {
                var activities = new List<object>();

                // OPTIMIZED: Chỉ SELECT các field cần thiết
                var recentPayments = await _db.Payments
                    .Where(p => p.PaymentDate.HasValue && p.Status == PaymentStatus.Success)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(3)
                    .Select(p => new
                    {
                        UserName = p.User != null ? p.User.FullName : "Unknown",
                        RoomName = p.Bill != null && p.Bill.Contract != null && p.Bill.Contract.Room != null 
                            ? p.Bill.Contract.Room.Name : "",
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate!.Value,
                        AvatarUrl = p.User != null ? p.User.AvatarUrl : null
                    })
                    .ToListAsync();

                foreach (var payment in recentPayments)
                {
                    activities.Add(new
                    {
                        type = "payment",
                        userName = payment.UserName,
                        description = $"Thanh toán tiền phòng {payment.RoomName}",
                        amount = $"+{payment.Amount / 1000000:F1} tr",
                        time = GetRelativeTime(payment.PaymentDate),
                        avatarUrl = payment.AvatarUrl
                    });
                }

                // OPTIMIZED: Chỉ SELECT các field cần thiết
                var recentBookings = await _db.Bookings
                    .Where(b => b.Status == BookingStatus.Pending)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(2)
                    .Select(b => new
                    {
                        UserName = b.User != null ? b.User.FullName : "Unknown",
                        RoomName = b.Room != null ? b.Room.Name : "",
                        CreatedAt = b.CreatedAt
                    })
                    .ToListAsync();

                foreach (var booking in recentBookings)
                {
                    activities.Add(new
                    {
                        type = "booking",
                        userName = booking.UserName,
                        description = $"Đặt phòng {booking.RoomName}",
                        time = GetRelativeTime(booking.CreatedAt),
                        badge = new { text = "Chờ duyệt", color = "yellow" }
                    });
                }

                return Ok(activities.Take(limit));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy hoạt động gần đây", error = ex.Message });
            }
        }

        private string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Vừa xong";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} giờ trước";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} ngày trước";

            return dateTime.ToString("dd/MM/yyyy");
        }
    }
}
