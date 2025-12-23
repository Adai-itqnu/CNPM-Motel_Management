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

        public BookingController(MotelManagementDbContext db, VNPayService vnpayService)
        {
            _db = db;
            _vnpayService = vnpayService;
        }

        // POST /api/booking/create-deposit
        [HttpPost("create-deposit")]
        public async Task<IActionResult> CreateDepositBooking([FromBody] CreateBookingDto dto)
        {
            Console.WriteLine("=== CREATE DEPOSIT BOOKING ===");
            Console.WriteLine($"Received: RoomId={dto.RoomId}, CheckInDate={dto.CheckInDate}, Phone={dto.ContactPhone}");
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User not authenticated");
                return Unauthorized("User not authenticated");
            }
            Console.WriteLine($"UserId: {userId}");


            // Check if user has CCCD
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                Console.WriteLine("User not found");
                return NotFound("User not found");
            }

            if (string.IsNullOrEmpty(user.IdCard))
            {
                Console.WriteLine("User missing CCCD");
                return BadRequest(new { 
                    requireUpdate = true,
                    message = "Vui lòng cập nhật CCCD trước khi đặt phòng" 
                });
            }
            
            // Validate CheckInDate
            if (dto.CheckInDate < DateTime.Today)
            {
                Console.WriteLine($"CheckInDate {dto.CheckInDate} is in the past");
                return BadRequest("Ngày nhận phòng phải từ hôm nay trở đi");
            }
            Console.WriteLine($"CheckInDate validation passed: {dto.CheckInDate}");


            // Check if room exists and is available
            var room = await _db.Rooms.FindAsync(dto.RoomId);
            if (room == null)
            {
                Console.WriteLine($"Room {dto.RoomId} not found");
                return NotFound("Room not found");
            }
            Console.WriteLine($"Room found: {room.Name}, Status: {room.Status}");

            if (room.Status != RoomStatus.Available)
            {
                Console.WriteLine($"Room {room.Name} is not available, status: {room.Status}");
                return BadRequest("Room is not available");
            }

            // Check if user already has pending booking
            var existingBooking = await _db.Bookings
                .Where(b => b.UserId == userId && b.Status == BookingStatus.Pending)
                .FirstOrDefaultAsync();

            if (existingBooking != null)
            {
                Console.WriteLine($"User already has pending booking: {existingBooking.Id}");
                return BadRequest("You already have a pending booking");
            }
            Console.WriteLine("No pending booking found, proceeding to create new booking");


            // Create booking
            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                RoomId = dto.RoomId,
                CheckInDate = dto.CheckInDate,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };


            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            // Create VNPAY payment URL
            var depositAmount = room.DepositAmount;
            var tmnCode = "729I87YR";
            var hashSecret = "ZKPI2R2IFEA4VIA1WMCMI65XQUMQHTWT";
            var returnUrl = "http://localhost:4200/payment/vnpay-return";
            
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
    }
}
