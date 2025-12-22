# QL_NhaTro_Server - Backend API

Backend API cho hệ thống quản lý nhà trọ, xây dựng bằng **ASP.NET Core 9.0** và **Entity Framework Core** với **MySQL**.

## 📋 Mục Lục

- [Yêu Cầu Hệ Thống](#yêu-cầu-hệ-thống)
- [Cài Đặt](#cài-đặt)
- [Cấu Hình](#cấu-hình)
- [Chạy Dự Án](#chạy-dự-án)
- [Cấu Trúc Dự Án](#cấu-trúc-dự-án)
- [API Endpoints](#api-endpoints)
- [Database](#database)
- [Xử Lý Lỗi Thường Gặp](#xử-lý-lỗi-thường-gặp)

---

## 🔧 Yêu Cầu Hệ Thống

### Bắt buộc:
- **.NET SDK 9.0** hoặc cao hơn
  - Tải tại: https://dotnet.microsoft.com/download
  - Kiểm tra: `dotnet --version`

- **MySQL 8.0** hoặc cao hơn
  - Tải tại: https://dev.mysql.com/downloads/mysql/
  - Hoặc dùng **XAMPP**: https://www.apachefriends.org/

### Tùy chọn (khuyến nghị):
- **dotnet-ef** (Entity Framework Core Tools)
- dotnet tool install --global dotnet-ef --version 9.0.0
- **Postman** hoặc **REST Client** (VS Code extension) để test API

---

## 📦 Cài Đặt

### 1. Clone hoặc Download dự án

```bash
cd C:\Users\ASUS\Downloads\CK_CNWEB\QL_NhaTro_Server
```

### 2. Restore NuGet packages

```bash
dotnet restore
```

### 3. Build project

```bash
dotnet build
```

---

## ⚙️ Cấu Hình

### 1. Cấu hình Database

Mở file `appsettings.json` và cập nhật connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;user=root;password=YOUR_PASSWORD;database=motel_management_db;"
  }
}
```

**Lưu ý:** Thay `YOUR_PASSWORD` bằng mật khẩu MySQL của bạn.

### 2. Tạo Database

#### Cách 1: Tự động khi chạy app (Khuyến nghị)

Thêm code sau vào `Program.cs` (sau dòng `var app = builder.Build();`):

```csharp
// Auto migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MotelManagementDbContext>();
    db.Database.Migrate();
}
```

Sau đó chỉ cần chạy:
```bash
dotnet run
```

#### Cách 2: Dùng EF Core Tools

```bash
# Cài dotnet-ef (nếu chưa có)
dotnet tool install --global dotnet-ef

# Tạo database từ migrations
dotnet ef database update
```

#### Cách 3: Tạo thủ công

```sql
-- Mở MySQL Command Line hoặc phpMyAdmin
CREATE DATABASE motel_management_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

Sau đó chạy migrations:
```bash
dotnet ef database update
```

---

## 🚀 Chạy Dự Án

### Development Mode

```bash
dotnet run
```

Hoặc với watch mode (auto-reload khi code thay đổi):

```bash
dotnet watch run
```

**Server sẽ chạy tại:**
- HTTP: `http://localhost:5001`
- (HTTPS có thể được cấu hình trong `Properties/launchSettings.json`)

### Production Mode

```bash
dotnet build --configuration Release
dotnet run --configuration Release
```

---

## 📁 Cấu Trúc Dự Án

```
QL_NhaTro_Server/
├── Controllers/           # API Controllers
│   └── AuthController.cs      # Authentication endpoints
│
├── Models/               # Database Entities
│   ├── User.cs               # User entity
│   ├── Room.cs               # Room entity
│   ├── Booking.cs            # Booking entity
│   ├── Contract.cs           # Contract entity
│   ├── Bill.cs               # Bill entity
│   ├── Payment.cs            # Payment entity
│   └── MotelManagementDbContext.cs  # EF Core DbContext
│
├── DTOs/                 # Data Transfer Objects
│   ├── UserDTOs.cs           # Login, Register DTOs
│   ├── RoomDTOs.cs           # Room DTOs
│   ├── BookingContractDTOs.cs # Booking & Contract DTOs
│   └── BillPaymentDTOs.cs    # Bill & Payment DTOs
│
├── Services/             # Business Logic
│   └── JwtService.cs         # JWT token generation
│
├── Migrations/           # EF Core Migrations
│   ├── 20251221170409_InitialCreate.cs
│   └── MotelManagementDbContextModelSnapshot.cs
│
├── Program.cs            # Entry point & configuration
├── appsettings.json      # Configuration file
└── README.md             # This file
```

---

## 🔌 API Endpoints

### Authentication (`/api/auth`)

#### **POST** `/api/auth/register`
Đăng ký tài khoản mới (User đầu tiên tự động là Admin)

**Request Body:**
```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "fullName": "string",
  "phone": "string"
}
```

**Response:**
```json
{
  "message": "Đăng ký thành công! Bạn là Admin đầu tiên của hệ thống.",
  "role": "admin"
}
```

#### **POST** `/api/auth/login`
Đăng nhập

**Request Body:**
```json
{
  "usernameOrEmail": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "guid",
    "username": "string",
    "email": "string",
    "fullName": "string",
    "role": "admin"
  }
}
```

---

## 🗄️ Database

### Schema

Database gồm 8 bảng chính:

1. **Users** - Quản lý người dùng (Admin/Tenant)
2. **Rooms** - Quản lý phòng
3. **Room_Amenities** - Tiện ích phòng
4. **Room_Images** - Hình ảnh phòng
5. **Bookings** - Đơn đặt phòng
6. **Contracts** - Hợp đồng thuê
7. **Bills** - Hóa đơn hàng tháng
8. **Payments** - Thanh toán (VNPAY)

### Entity Framework Core

Dự án sử dụng **Code-First Approach**:
- ✅ Định nghĩa Models → EF Core tự động tạo database
- ✅ Khi sửa Models → Tạo migration mới
- ✅ Không cần viết SQL thủ công

### Migrations

#### Tạo migration mới (khi sửa Models)

```bash
dotnet ef migrations add TenMoTa
```

Ví dụ:
```bash
dotnet ef migrations add AddAddressToUser
```

#### Áp dụng migration vào database

```bash
dotnet ef database update
```

#### Rollback migration

```bash
# Rollback về migration trước đó
dotnet ef database update TenMigrationTruocDo

# Xem danh sách migrations
dotnet ef migrations list
```

#### Xóa migration cuối (chưa apply)

```bash
dotnet ef migrations remove
```

### Reset Database

#### Cách 1: Qua phpMyAdmin
1. Mở http://localhost/phpmyadmin
2. Chọn database `motel_management_db`
3. Click "Drop the database"
4. Chạy: `dotnet ef database update`

#### Cách 2: Qua MySQL Command
```bash
mysql -u root -pYOUR_PASSWORD -e "DROP DATABASE motel_management_db; CREATE DATABASE motel_management_db;"
dotnet ef database update
```

#### Cách 3: Qua dotnet-ef
```bash
dotnet ef database drop --force
dotnet ef database update
```

---

## 🧪 Testing API

### Sử dụng REST Client (VS Code)

1. Cài extension **REST Client**
2. Tạo file `test.http`:

```http
### Register first user
POST http://localhost:5001/api/auth/register
Content-Type: application/json

{
  "username": "admin",
  "email": "admin@test.com",
  "password": "admin123",
  "fullName": "Admin User",
  "phone": "0123456789"
}

### Login
POST http://localhost:5001/api/auth/login
Content-Type: application/json

{
  "usernameOrEmail": "admin",
  "password": "admin123"
}
```

3. Click "Send Request" để test

### Sử dụng cURL

```bash
# Register
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","email":"admin@test.com","password":"admin123","fullName":"Admin User","phone":"0123456789"}'

# Login
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usernameOrEmail":"admin","password":"admin123"}'
```

---

## ⚠️ Xử Lý Lỗi Thường Gặp

### 1. "Could not connect to database"

**Nguyên nhân:** MySQL chưa chạy hoặc connection string sai

**Giải pháp:**
- Kiểm tra MySQL đã chạy (XAMPP/MySQL Service)
- Kiểm tra username/password trong `appsettings.json`
- Kiểm tra port (mặc định 3306)

### 2. "Unknown database 'motel_management_db'"

**Nguyên nhân:** Database chưa được tạo

**Giải pháp:**
```bash
dotnet ef database update
```

### 3. "Unknown column 'AvatarUrl'"

**Nguyên nhân:** Database schema không khớp với Models

**Giải pháp:**
```bash
# Reset database
dotnet ef database drop --force
dotnet ef database update
```

### 4. "Port 5001 already in use"

**Nguyên nhân:** Process cũ vẫn đang chạy

**Giải pháp:**
- Tắt terminal cũ (Ctrl+C)
- Hoặc kill process qua Task Manager
- Hoặc đổi port trong `Properties/launchSettings.json`

### 5. "dotnet-ef command not found"

**Nguyên nhân:** EF Core Tools chưa được cài

**Giải pháp:**
```bash
dotnet tool install --global dotnet-ef
```

---

## 🔐 Authentication

### JWT Token

Hệ thống sử dụng **JWT (JSON Web Token)** để xác thực:

1. User đăng nhập → Nhận token
2. Gửi token trong header cho các API khác:
   ```
   Authorization: Bearer <token>
   ```

### Roles

- **Admin**: Quản lý phòng, duyệt booking, tạo hóa đơn
- **Tenant**: Đặt phòng, xem hóa đơn, thanh toán

**Logic đặc biệt:** User đăng ký đầu tiên tự động là Admin!

---

## 📝 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;user=root;password=YOUR_PASSWORD;database=motel_management_db;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWT_MinimumLengthIs32Characters_ChangeInProduction",
    "Issuer": "MotelManagementAPI",
    "Audience": "MotelManagementClient",
    "ExpiryInMinutes": "1440"
  }
}
```

**Lưu ý Security:**
- ⚠️ Đổi `SecretKey` trong production!
- ⚠️ Không commit file có password thật lên Git!
- ✅ Dùng User Secrets hoặc Environment Variables trong production

---

## 🚀 Deployment

### Publish

```bash
dotnet publish --configuration Release --output ./publish
```

### Run Production Build

```bash
cd publish
dotnet QL_NhaTro_Server.dll
```

---

## 📞 Support

Nếu gặp vấn đề:
1. Kiểm tra console logs
2. Kiểm tra MySQL logs
3. Đọc lại phần [Xử Lý Lỗi Thường Gặp](#xử-lý-lỗi-thường-gặp)

---

## 📄 License

This project is for educational purposes.

---

**Happy Coding! 🎉**
