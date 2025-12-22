import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './booking.component.html',
  styleUrl: './booking.component.css'
})
export class BookingComponent {
  // Đặt phòng
  // Gửi yêu cầu đặt phòng
  // Chọn ngày vào ở
  // Thanh toán tiền cọc qua VNPay
  // Xử lý callback từ VNPay
}
