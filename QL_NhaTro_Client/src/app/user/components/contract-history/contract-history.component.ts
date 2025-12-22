import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-contract-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contract-history.component.html',
  styleUrl: './contract-history.component.css'
})
export class ContractHistoryComponent {
  // Lịch sử hợp đồng
  // Xem danh sách hợp đồng đã ký
  // Chi tiết từng hợp đồng
  // Trạng thái: Đang hoạt động, Đã kết thúc
}
