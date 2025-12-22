import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-room-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './room-list.component.html',
  styleUrl: './room-list.component.css'
})
export class RoomListComponent {
  // Danh sách phòng trống
  // Xem danh sách phòng có sẵn
  // Tìm kiếm phòng theo tiêu chí
  // Filter theo giá, diện tích, tiện nghi
}
