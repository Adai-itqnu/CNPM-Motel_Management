using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.Models;

namespace QL_NhaTro_Server.Services
{
    public interface IBillGenerationService
    {
        Task<List<Bill>> GenerateMonthlyBillsAsync(int month, int year);
        Task<Bill?> GenerateBillForContractAsync(string contractId, int month, int year);
        decimal CalculateProRataRoomPrice(decimal monthlyPrice, int daysRented, int daysInMonth);
    }

    public class BillGenerationService : IBillGenerationService
    {
        private readonly MotelManagementDbContext _db;
        private readonly INotificationService _notificationService;

        public BillGenerationService(MotelManagementDbContext db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Tạo hóa đơn hàng tháng cho tất cả hợp đồng đang hoạt động
        /// </summary>
        public async Task<List<Bill>> GenerateMonthlyBillsAsync(int month, int year)
        {
            var generatedBills = new List<Bill>();

            // Lấy tất cả contracts đang Active
            var activeContracts = await _db.Contracts
                .Include(c => c.Room)
                .Include(c => c.User)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

            foreach (var contract in activeContracts)
            {
                // Kiểm tra xem đã có hóa đơn cho tháng này chưa
                var existingBill = await _db.Bills
                    .FirstOrDefaultAsync(b => b.ContractId == contract.Id && b.Month == month && b.Year == year);

                if (existingBill != null)
                    continue; // Đã có hóa đơn rồi

                var bill = await GenerateBillForContractAsync(contract.Id, month, year);
                if (bill != null)
                {
                    generatedBills.Add(bill);
                }
            }

            return generatedBills;
        }

        /// <summary>
        /// Tạo hóa đơn cho một hợp đồng cụ thể
        /// </summary>
        public async Task<Bill?> GenerateBillForContractAsync(string contractId, int month, int year)
        {
            var contract = await _db.Contracts
                .Include(c => c.Room)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == contractId);

            if (contract == null || contract.Status != ContractStatus.Active)
                return null;

            // Kiểm tra xem đã có hóa đơn cho tháng này chưa
            var existingBill = await _db.Bills
                .FirstOrDefaultAsync(b => b.ContractId == contractId && b.Month == month && b.Year == year);

            if (existingBill != null)
                return existingBill; // Trả về hóa đơn đã tồn tại

            // Tính số ngày trong tháng
            var daysInMonth = DateTime.DaysInMonth(year, month);
            
            // Tính số ngày thuê thực tế (pro-rata)
            var daysRented = CalculateDaysRented(contract.StartDate, month, year, daysInMonth);

            // Tính tiền phòng pro-rata
            var roomPrice = CalculateProRataRoomPrice(contract.MonthlyPrice, daysRented, daysInMonth);

            // Lấy chỉ số điện/nước từ hóa đơn tháng trước (nếu có)
            var lastBill = await _db.Bills
                .Where(b => b.ContractId == contractId)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .FirstOrDefaultAsync();

            var electricityOldIndex = lastBill?.ElectricityNewIndex ?? contract.ElectricityStartIndex;
            var waterOldIndex = lastBill?.WaterNewIndex ?? contract.WaterStartIndex;

            // Tạo hóa đơn mới
            var bill = new Bill
            {
                Id = Guid.NewGuid().ToString(),
                ContractId = contract.Id,
                RoomId = contract.RoomId,
                UserId = contract.UserId,
                Month = month,
                Year = year,
                
                // Pro-rata
                DaysInMonth = daysInMonth,
                DaysRented = daysRented,
                RoomPrice = roomPrice,
                
                // Điện (admin sẽ điền sau)
                ElectricityOldIndex = electricityOldIndex,
                ElectricityNewIndex = 0, // Admin điền
                ElectricityPrice = contract.Room.ElectricityPrice,
                ElectricityTotal = 0, // Sẽ tính khi admin điền
                
                // Nước (admin sẽ điền sau)
                WaterOldIndex = waterOldIndex,
                WaterNewIndex = 0, // Admin điền
                WaterPrice = contract.Room.WaterPrice,
                WaterTotal = 0, // Sẽ tính khi admin điền
                
                OtherFees = 0,
                TotalAmount = roomPrice, // Tạm thời chỉ có tiền phòng
                
                Status = BillStatus.Pending,
                IsSent = false, // Chưa gửi cho người thuê
                // Hạn thanh toán: ngày 7 tháng tiếp theo (1 tuần sau đầu tháng mới)
                DueDate = month == 12 
                    ? new DateTime(year + 1, 1, 7) 
                    : new DateTime(year, month + 1, 7),
                
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Bills.Add(bill);
            await _db.SaveChangesAsync();

            return bill;
        }

        /// <summary>
        /// Tính tiền phòng theo số ngày ở (pro-rata)
        /// Ví dụ: Phòng 3 triệu/tháng, ở 15 ngày = 1.5 triệu
        /// </summary>
        public decimal CalculateProRataRoomPrice(decimal monthlyPrice, int daysRented, int daysInMonth)
        {
            if (daysInMonth <= 0) return monthlyPrice;
            return Math.Round(monthlyPrice * daysRented / daysInMonth, 0);
        }

        /// <summary>
        /// Tính số ngày thuê thực tế trong tháng
        /// </summary>
        private int CalculateDaysRented(DateTime contractStartDate, int month, int year, int daysInMonth)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = new DateTime(year, month, daysInMonth);

            // Nếu hợp đồng bắt đầu sau tháng này, không có ngày thuê
            if (contractStartDate > lastDayOfMonth)
                return 0;

            // Nếu hợp đồng bắt đầu trước tháng này, thuê cả tháng
            if (contractStartDate <= firstDayOfMonth)
                return daysInMonth;

            // Hợp đồng bắt đầu trong tháng này, tính từ ngày bắt đầu đến cuối tháng
            return daysInMonth - contractStartDate.Day + 1;
        }
    }
}
