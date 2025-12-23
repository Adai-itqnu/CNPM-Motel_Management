using System.ComponentModel.DataAnnotations;

namespace QL_NhaTro_Server.DTOs
{
    // Dashboard Statistics Summary
    public class DashboardStatsDto
    {
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int PendingBookings { get; set; }
    }

    // Room Status Breakdown
    public class RoomStatusDto
    {
        public int Occupied { get; set; }
        public int Available { get; set; }
        public int Maintenance { get; set; }
        public int Total { get; set; }
    }

    // Revenue Chart Data
    public class RevenueChartDto
    {
        public List<MonthlyRevenueDto> MonthlyData { get; set; } = new();
    }

    public class MonthlyRevenueDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }
}
