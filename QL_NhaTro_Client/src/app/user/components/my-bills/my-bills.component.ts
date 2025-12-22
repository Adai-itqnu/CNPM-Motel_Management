import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-my-bills',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-bills.component.html',
  styleUrl: './my-bills.component.css'
})
export class MyBillsComponent {
  // Hóa đơn của tôi
  // Xem danh sách hóa đơn hàng tháng
  // Chi tiết các khoản phí: tiền phòng, điện, nước, phí khác
  // Thanh toán hóa đơn qua VNPay
  // Lịch sử thanh toán
}
