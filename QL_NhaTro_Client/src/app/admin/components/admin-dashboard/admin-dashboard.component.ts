import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent {
  // Dashboard tổng quan cho Admin
  // Hiển thị thống kê tổng quan: số phòng, tình trạng phòng, doanh thu, booking chờ duyệt
}
