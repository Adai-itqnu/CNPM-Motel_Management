import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-booking-approval',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './booking-approval.component.html',
  styleUrl: './booking-approval.component.css'
})
export class BookingApprovalComponent {
  // Phê duyệt đặt phòng
  // Xem danh sách yêu cầu đặt phòng
  // Phê duyệt hoặc từ chối đặt phòng
}
