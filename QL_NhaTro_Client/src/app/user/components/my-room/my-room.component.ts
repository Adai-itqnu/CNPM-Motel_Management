import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-my-room',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-room.component.html',
  styleUrl: './my-room.component.css'
})
export class MyRoomComponent {
  // Phòng của tôi
  // Xem thông tin phòng đang thuê
  // Thông tin hợp đồng hiện tại
  // Thông tin liên hệ chủ nhà
}
