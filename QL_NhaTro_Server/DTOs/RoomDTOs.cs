using System.ComponentModel.DataAnnotations;

namespace QL_NhaTro_Server.DTOs
{
    public class CreateRoomDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? RoomType { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public decimal? DepositAmount { get; set; }

        public decimal? ElectricityPrice { get; set; }

        public decimal? WaterPrice { get; set; }

        public string? Description { get; set; }

        public decimal? Area { get; set; }

        public int Floor { get; set; } = 1;

        public List<string>? Amenities { get; set; }
    }

    public class UpdateRoomDto
    {
        public string? Name { get; set; }

        public string? RoomType { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        public decimal? DepositAmount { get; set; }

        public decimal? ElectricityPrice { get; set; }

        public decimal? WaterPrice { get; set; }

        public string? Description { get; set; }

        public decimal? Area { get; set; }

        public int? Floor { get; set; }

        public string? Status { get; set; }
    }

    public class AddRoomImageDto
    {
        [Required]
        public string RoomId { get; set; } = string.Empty;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public string Filename { get; set; } = string.Empty;

        public string ContentType { get; set; } = "image/jpeg";

        public bool IsPrimary { get; set; } = false;
    }

    public class RoomResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? RoomType { get; set; }
        public decimal Price { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal ElectricityPrice { get; set; }
        public decimal WaterPrice { get; set; }
        public string? Description { get; set; }
        public decimal? Area { get; set; }
        public int Floor { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CurrentUserId { get; set; }
        public List<string> Amenities { get; set; } = new();
        public List<RoomImageDto> Images { get; set; } = new();
    }

    public class RoomImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
