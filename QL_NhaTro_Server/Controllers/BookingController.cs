using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.DTOs;
using QL_NhaTro_Server.Models;
using QL_NhaTro_Server.Services;
using System.Security.Claims;

namespace QL_NhaTro_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly MotelManagementDbContext _db;
        private readonly VNPayService _vnpayService;
        private readonly IConfiguration _configuration;

        public BookingController(MotelManagementDbContext db, VNPayService vnpayService, IConfiguration configuration)
        {
            _db = db;
            _vnpayService = vnpayService;
            _configuration = configuration;
        }

        // POST /api/booking/create-deposit
        [HttpPost("create-deposit")]
        public async Task<IActionResult> CreateDepositBooking([FromBody] CreateBookingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            // Check if user has CCCD
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (string.IsNullOrEmpty(user.IdCard))
            {
                return BadRequest(new { 
                    requireUpdate = true,
                    message = "Vui lòng cập nhật CCCD trước khi đặt phòng" 
                });
            }
            
            // Validate CheckInDate
            if (dto.CheckInDate < DateTime.Today)
            {
                return BadRequest("Ngày nhận phòng phải từ hôm nay trở đi");
            }

            // Check if room exists and is available
            var room = await _db.Rooms.FindAsync(dto.RoomId);
            if (room == null)
            {
                return NotFound("Room not found");
            }

            if (room.Status != RoomStatus.Available)
            {
                return BadRequest("Room is not available");
            }

            // Check if user already has pending booking
            var existingBooking = await _db.Bookings
                .Where(b => b.UserId == userId && b.Status == BookingStatus.Pending)
                .FirstOrDefaultAsync();

            if (existingBooking != null)
            {
                return BadRequest("You already have a pending booking");
            }

            // Get deposit amount from room (if 0, use room price as default)
            var depositAmount = room.DepositAmount > 0 ? room.DepositAmount : room.Price;

            // Create booking with deposit amount
            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                RoomId = dto.RoomId,
                CheckInDate = dto.CheckInDate,
                DepositAmount = depositAmount,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            // Create VNPAY payment URL from configuration
            var tmnCode = _configuration["VNPaySettings:TmnCode"]!;
            var hashSecret = _configuration["VNPaySettings:HashSecret"]!;
            var returnUrl = _configuration["VNPaySettings:ReturnUrl"]!;
            
            var paymentUrl = _vnpayService.CreatePaymentUrl(
                orderId: booking.Id,
                amount: depositAmount,
                orderInfo: $"Dat coc phong {room.Name}",
                returnUrl: returnUrl,
                tmnCode: tmnCode,
                hashSecret: hashSecret
            );

            return Ok(new BookingResponseDto
            {
                BookingId = booking.Id,
                PaymentUrl = paymentUrl,
                DepositAmount = depositAmount
            });
        }

        // GET /api/booking/my-bookings
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var bookings = await _db.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Room)
                    .ThenInclude(r => r.Images)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.Id,
                    b.Status,
                    b.CreatedAt,
                    Room = new
                    {
                        b.Room.Id,
                        b.Room.Name,
                        b.Room.RoomType,
                        b.Room.Floor,
                        b.Room.Area,
                        b.Room.DepositAmount,
                        Images = b.Room.Images.Select(i => new
                        {
                            i.Id,
                            i.ImageUrl,
                            i.IsPrimary
                        }).ToList()
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // POST /api/booking/{id}/check-in
        [HttpPost("{id}/check-in")]
        public async Task<IActionResult> CheckIn(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var booking = await _db.Bookings
                .Include(b => b.Room)
                .Include(b => b.Contract)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy booking" });
            }

            // Verify ownership
            if (booking.UserId != userId)
            {
                return Forbid("Bạn không có quyền thực hiện thao tác này");
            }

            // Verify booking status
            if (booking.Status != BookingStatus.Approved)
            {
                return BadRequest(new { message = "Booking chưa được thanh toán hoặc đã bị hủy" });
            }

            // Verify contract exists and is Draft
            if (booking.Contract == null || booking.Contract.Status != ContractStatus.Draft)
            {
                return BadRequest(new { message = "Không tìm thấy hợp đồng hoặc hợp đồng không hợp lệ" });
            }

            // Verify check-in date
            if (booking.CheckInDate > DateTime.Today)
            {
                return BadRequest(new { message = $"Chưa đến ngày nhận phòng ({booking.CheckInDate:dd/MM/yyyy})" });
            }

            // Activate contract
            booking.Contract.Status = ContractStatus.Active;
            booking.Contract.UpdatedAt = DateTime.Now;

            // Update room to Occupied and link contract
            if (booking.Room != null)
            {
                booking.Room.Status = RoomStatus.Occupied;
                booking.Room.CurrentUserId = userId;
                booking.Room.CurrentContractId = booking.Contract.Id;
                booking.Room.UpdatedAt = DateTime.Now;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Nhận phòng thành công! Hợp đồng đã được kích hoạt.",
                contractId = booking.Contract.Id,
                roomName = booking.Room?.Name
            });
        }

        // GET /api/booking/my-rooms (rooms where user has active contract)
        [HttpGet("my-rooms")]
        public async Task<IActionResult> GetMyRooms()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var rooms = await _db.Contracts
                .Where(c => c.UserId == userId && (c.Status == ContractStatus.Active || c.Status == ContractStatus.Draft))
                .Include(c => c.Room)
                    .ThenInclude(r => r.Images)
                .Include(c => c.Booking)
                .Select(c => new
                {
                    ContractId = c.Id,
                    ContractStatus = c.Status.ToString(),
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    MonthlyPrice = c.MonthlyPrice,
                    CanCheckIn = c.Status == ContractStatus.Draft && c.Booking != null && c.Booking.CheckInDate <= DateTime.Today,
                    CheckInDate = c.Booking != null ? c.Booking.CheckInDate : (DateTime?)null,
                    BookingId = c.BookingId,
                    Room = new
                    {
                        c.Room.Id,
                        c.Room.Name,
                        c.Room.RoomType,
                        c.Room.Floor,
                        c.Room.Area,
                        c.Room.Price,
                        c.Room.Status,
                        Images = c.Room.Images.Select(i => new
                        {
                            i.Id,
                            i.ImageUrl,
                            i.IsPrimary
                        }).ToList()
                    }
                })
                .ToListAsync();

            return Ok(rooms);
        }
    }
}
