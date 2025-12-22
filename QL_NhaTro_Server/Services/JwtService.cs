using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using QL_NhaTro_Server.Models;

namespace QL_NhaTro_Server.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }
        
        public string GenerateToken(User user)
        {
            // 1️⃣ Lấy secret key từ appsettings.json
            var secretKey = _config["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not found");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // 2️⃣ Tạo danh sách claim (thông tin lưu trong token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // 3️⃣ Tạo token
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2), // token sống 2 giờ
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
