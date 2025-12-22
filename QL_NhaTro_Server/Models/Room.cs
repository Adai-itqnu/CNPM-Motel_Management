using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_NhaTro_Server.Models
{
    public enum RoomStatus
    {
        Available,
        Occupied,
        Maintenance,
        Reserved
    }

    [Table("Rooms")]
    public class Room
    {
        [Key]
        [MaxLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? RoomType { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal DepositAmount { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal ElectricityPrice { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal WaterPrice { get; set; } = 0;

        [Column(TypeName = "text")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Area { get; set; }

        public int Floor { get; set; } = 1;

        [Required]
        public RoomStatus Status { get; set; } = RoomStatus.Available;

        [MaxLength(50)]
        public string? CurrentTenantId { get; set; }

        [MaxLength(50)]
        public string? CurrentContractId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CurrentTenantId")]
        public virtual User? CurrentTenant { get; set; }

        public virtual ICollection<RoomAmenity> Amenities { get; set; } = new List<RoomAmenity>();
        public virtual ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
    }

    [Table("Room_Amenities")]
    public class RoomAmenity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoomId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AmenityName { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } = null!;
    }

    [Table("Room_Images")]
    public class RoomImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoomId { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Filename { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ContentType { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;

        // Navigation property
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } = null!;
    }
}
