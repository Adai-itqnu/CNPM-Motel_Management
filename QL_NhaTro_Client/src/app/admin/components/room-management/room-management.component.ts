import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-room-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './room-management.component.html',
  styleUrl: './room-management.component.css'
})
export class RoomManagementComponent {
  // Quản lý phòng
  // CRUD: Thêm, sửa, xóa phòng
  // Cập nhật thông tin: giá, diện tích, tiện nghi, hình ảnh
  // Thống kê: tỷ lệ lấp đầy, phòng trống, phòng bảo trì
}
