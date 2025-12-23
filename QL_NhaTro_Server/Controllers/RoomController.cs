using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.DTOs;
using QL_NhaTro_Server.Models;

namespace QL_NhaTro_Server.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomController : ControllerBase
    {
        private readonly MotelManagementDbContext _db;

        public RoomController(MotelManagementDbContext db)
        {
            _db = db;
        }

        // GET: api/rooms - Lấy danh sách phòng với tìm kiếm, lọc, phân trang
        [HttpGet]
        public async Task<IActionResult> GetRooms(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int? floor,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _db.Rooms
                .AsNoTracking() // CRITICAL: Không track thay đổi - nhanh hơn 30%!
                .Include(r => r.Amenities)
                .Include(r => r.Images)
                .Include(r => r.CurrentUser)
                .AsQueryable();

            // Tìm kiếm theo tên hoặc mô tả
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => 
                    r.Name.Contains(search) || 
                    (r.Description != null && r.Description.Contains(search)));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RoomStatus>(status, true, out var roomStatus))
            {
                query = query.Where(r => r.Status == roomStatus);
            }

            // Lọc theo tầng
            if (floor.HasValue)
            {
                query = query.Where(r => r.Floor == floor.Value);
            }

            // Lọc theo giá
            if (minPrice.HasValue)
            {
                query = query.Where(r => r.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(r => r.Price <= maxPrice.Value);
            }

            // OPTIMIZED: Count trước khi Include() - nhanh hơn!
            var countQuery = query.Select(r => r.Id);
            var total = await countQuery.CountAsync();

            // Phân trang
            var rooms = await query
                .OrderBy(r => r.Floor)
                .ThenBy(r => r.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RoomResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    RoomType = r.RoomType,
                    Price = r.Price,
                    DepositAmount = r.DepositAmount,
                    ElectricityPrice = r.ElectricityPrice,
                    WaterPrice = r.WaterPrice,
                    Description = r.Description,
                    Area = r.Area,
                    Floor = r.Floor,
                    Status = r.Status.ToString(),
                    CurrentUserId = r.CurrentUserId,
                    Amenities = r.Amenities.Select(a => a.AmenityName).ToList(),
                    Images = r.Images.Select(i => new RoomImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        Filename = i.Filename,
                        IsPrimary = i.IsPrimary
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                data = rooms,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        // GET: api/rooms/{id} - Lấy chi tiết phòng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom(string id)
        {
            var room = await _db.Rooms
                .Include(r => r.Amenities)
                .Include(r => r.Images)
                .Include(r => r.CurrentUser)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng" });

            var response = new RoomResponseDto
            {
                Id = room.Id,
                Name = room.Name,
                RoomType = room.RoomType,
                Price = room.Price,
                DepositAmount = room.DepositAmount,
                ElectricityPrice = room.ElectricityPrice,
                WaterPrice = room.WaterPrice,
                Description = room.Description,
                Area = room.Area,
                Floor = room.Floor,
                Status = room.Status.ToString(),
                CurrentUserId = room.CurrentUserId,
                Amenities = room.Amenities.Select(a => a.AmenityName).ToList(),
                Images = room.Images.Select(i => new RoomImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    Filename = i.Filename,
                    IsPrimary = i.IsPrimary
                }).ToList()
            };

            return Ok(response);
        }

        // POST: api/rooms - Tạo phòng mới
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto dto)
        {
            // Kiểm tra tên phòng đã tồn tại
            if (await _db.Rooms.AnyAsync(r => r.Name == dto.Name))
                return BadRequest(new { message = "Tên phòng đã tồn tại" });

            var room = new Room
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                RoomType = dto.RoomType,
                Price = dto.Price,
                DepositAmount = dto.DepositAmount ?? (dto.Price * 1), // Mặc định = 1 tháng tiền phòng
                ElectricityPrice = dto.ElectricityPrice ?? 3500,
                WaterPrice = dto.WaterPrice ?? 20000,
                Description = dto.Description,
                Area = dto.Area,
                Floor = dto.Floor,
                Status = RoomStatus.Available,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Rooms.Add(room);

            // Thêm tiện ích
            if (dto.Amenities != null && dto.Amenities.Any())
            {
                foreach (var amenity in dto.Amenities)
                {
                    _db.RoomAmenities.Add(new RoomAmenity
                    {
                        RoomId = room.Id,
                        AmenityName = amenity
                    });
                }
            }

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, new { message = "Tạo phòng thành công", roomId = room.Id });
        }

        // PUT: api/rooms/{id} - Cập nhật thông tin phòng
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(string id, [FromBody] UpdateRoomDto dto)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng" });

            // Kiểm tra tên phòng mới (nếu có)
            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != room.Name)
            {
                if (await _db.Rooms.AnyAsync(r => r.Name == dto.Name && r.Id != id))
                    return BadRequest(new { message = "Tên phòng đã tồn tại" });
                room.Name = dto.Name;
            }

            // Cập nhật các trường
            if (!string.IsNullOrEmpty(dto.RoomType)) room.RoomType = dto.RoomType;
            if (dto.Price.HasValue) room.Price = dto.Price.Value;
            if (dto.DepositAmount.HasValue) room.DepositAmount = dto.DepositAmount.Value;
            if (dto.ElectricityPrice.HasValue) room.ElectricityPrice = dto.ElectricityPrice.Value;
            if (dto.WaterPrice.HasValue) room.WaterPrice = dto.WaterPrice.Value;
            if (dto.Description != null) room.Description = dto.Description;
            if (dto.Area.HasValue) room.Area = dto.Area.Value;
            if (dto.Floor.HasValue) room.Floor = dto.Floor.Value;
            
            if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<RoomStatus>(dto.Status, true, out var status))
                room.Status = status;

            room.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Cập nhật phòng thành công" });
        }

        // DELETE: api/rooms/{id} - Xóa phòng
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(string id)
        {
            var room = await _db.Rooms
                .Include(r => r.Contracts)
                .Include(r => r.Bookings)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng" });

            // Kiểm tra xem phòng có hợp đồng đang hoạt động không
            if (room.Contracts.Any(c => c.Status == ContractStatus.Active))
                return BadRequest(new { message = "Không thể xóa phòng đang có hợp đồng hoạt động" });

            // Kiểm tra booking đang pending
            if (room.Bookings.Any(b => b.Status == BookingStatus.Pending))
                return BadRequest(new { message = "Không thể xóa phòng đang có booking chờ xử lý" });

            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Xóa phòng thành công" });
        }

        // POST: api/rooms/{id}/amenities - Thêm tiện ích
        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/amenities")]
        public async Task<IActionResult> AddAmenity(string id, [FromBody] AddAmenityDto dto)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng" });

            var amenity = new RoomAmenity
            {
                RoomId = id,
                AmenityName = dto.AmenityName
            };

            _db.RoomAmenities.Add(amenity);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Thêm tiện ích thành công" });
        }

        // DELETE: api/rooms/amenities/{amenityId} - Xóa tiện ích
        [Authorize(Roles = "Admin")]
        [HttpDelete("amenities/{amenityId}")]
        public async Task<IActionResult> DeleteAmenity(int amenityId)
        {
            var amenity = await _db.RoomAmenities.FindAsync(amenityId);
            if (amenity == null)
                return NotFound(new { message = "Không tìm thấy tiện ích" });

            _db.RoomAmenities.Remove(amenity);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Xóa tiện ích thành công" });
        }

        // POST: api/rooms/{id}/images - Thêm hình ảnh
        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/images")]
        public async Task<IActionResult> AddImage(string id, [FromForm] IFormFile file)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng" });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File không hợp lệ" });

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { message = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp)" });

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Kích thước file không được vượt quá 5MB" });

            try
            {
                // Create uploads directory if not exists
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "rooms");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create image record in database
                var imageUrl = $"/uploads/rooms/{uniqueFileName}";
                var image = new RoomImage
                {
                    RoomId = id,
                    ImageUrl = imageUrl,
                    Filename = file.FileName,
                    ContentType = file.ContentType,
                    IsPrimary = false // Set first image as primary if no images exist
                };

                // Check if this is the first image
                var hasImages = await _db.RoomImages.AnyAsync(i => i.RoomId == id);
                if (!hasImages)
                    image.IsPrimary = true;

                _db.RoomImages.Add(image);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Thêm hình ảnh thành công", imageId = image.Id, imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lưu file", error = ex.Message });
            }
        }

        // DELETE: api/rooms/images/{imageId} - Xóa hình ảnh
        [Authorize(Roles = "Admin")]
        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _db.RoomImages.FindAsync(imageId);
            if (image == null)
                return NotFound(new { message = "Không tìm thấy hình ảnh" });

            try
            {
                // Delete physical file
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                // Delete from database
                _db.RoomImages.Remove(image);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Xóa hình ảnh thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa hình ảnh", error = ex.Message });
            }
        }
    }
    // DTO bổ sung
    public class AddAmenityDto
    {
        public string AmenityName { get; set; } = string.Empty;
    }
}