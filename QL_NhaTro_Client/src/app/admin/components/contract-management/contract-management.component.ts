import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-contract-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contract-management.component.html',
  styleUrl: './contract-management.component.css'
})
export class ContractManagementComponent {
  // Quản lý hợp đồng
  // Tạo hợp đồng mới sau khi booking được duyệt
  // Kết thúc hợp đồng (termination)
  // Xem danh sách hợp đồng đang hoạt động
}
