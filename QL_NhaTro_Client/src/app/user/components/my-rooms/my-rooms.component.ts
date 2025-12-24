import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BookingService } from '../../../services/booking.service';

interface MyRoom {
  contractId: string;
  contractStatus: string;
  startDate: string;
  endDate: string;
  monthlyPrice: number;
  canCheckIn: boolean;
  checkInDate: string | null;
  bookingId: string;
  room: {
    id: string;
    name: string;
    roomType: string;
    floor: number;
    area: number;
    price: number;
    status: string;
    images: { id: string; imageUrl: string; isPrimary: boolean }[];
  };
}

@Component({
  selector: 'app-my-rooms',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './my-rooms.component.html',
  styleUrls: ['./my-rooms.component.css']
})
export class MyRoomsComponent implements OnInit {
  rooms: MyRoom[] = [];
  isLoading = true;
  error = '';
  checkingIn = false;

  constructor(private bookingService: BookingService) {}

  ngOnInit(): void {
    this.loadMyRooms();
  }

  loadMyRooms(): void {
    this.isLoading = true;
    this.bookingService.getMyRooms().subscribe({
      next: (data) => {
        this.rooms = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Không thể tải danh sách phòng';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  checkIn(bookingId: string): void {
    if (this.checkingIn) return;
    
    if (!confirm('Xác nhận nhận phòng? Hợp đồng sẽ được kích hoạt.')) {
      return;
    }

    this.checkingIn = true;
    this.bookingService.checkIn(bookingId).subscribe({
      next: (response) => {
        alert(response.message || 'Nhận phòng thành công!');
        this.loadMyRooms(); // Reload to update status
        this.checkingIn = false;
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra khi nhận phòng');
        this.checkingIn = false;
      }
    });
  }

  getRoomImage(room: MyRoom['room']): string {
    const baseUrl = 'http://localhost:5001';
    const primaryImage = room.images?.find(img => img.isPrimary);
    let imageUrl = '';
    
    if (primaryImage) {
      imageUrl = primaryImage.imageUrl;
    } else if (room.images && room.images.length > 0) {
      imageUrl = room.images[0].imageUrl;
    }
    
    if (!imageUrl) {
      return 'assets/images/room-placeholder.jpg';
    }
    
    // If URL is relative (starts with /), prepend backend URL
    if (imageUrl.startsWith('/')) {
      return baseUrl + imageUrl;
    }
    
    // If it's already a full URL, return as-is
    return imageUrl;
  }

  getStatusLabel(status: string): string {
    const labels: { [key: string]: string } = {
      'Draft': 'Chờ nhận phòng',
      'Active': 'Đang thuê',
      'Expired': 'Đã hết hạn',
      'Terminated': 'Đã chấm dứt',
      'Cancelled': 'Đã hủy'
    };
    return labels[status] || status;
  }

  getStatusClass(status: string): string {
    const classes: { [key: string]: string } = {
      'Draft': 'status-draft',
      'Active': 'status-active',
      'Expired': 'status-expired',
      'Terminated': 'status-terminated',
      'Cancelled': 'status-cancelled'
    };
    return classes[status] || '';
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    return date.toLocaleDateString('vi-VN');
  }
}
