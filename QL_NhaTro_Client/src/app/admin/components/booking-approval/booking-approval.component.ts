import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../services/admin.service';

interface Booking {
  id: string;
  roomId: string;
  roomName: string;
  userId: string;
  userName: string;
  userEmail: string;
  userPhone: string;
  checkInDate: string;
  depositAmount: number;
  depositStatus: string;
  status: string;
  adminNote: string | null;
  createdAt: string;
  updatedAt: string;
}

@Component({
  selector: 'app-booking-approval',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './booking-approval.component.html',
  styleUrls: ['./booking-approval.component.css']
})
export class BookingApprovalComponent implements OnInit {
  bookings: Booking[] = [];
  isLoading = true;
  error = '';
  filterStatus = 'all';

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  loadBookings(): void {
    this.isLoading = true;
    this.adminService.getAllBookings().subscribe({
      next: (data) => {
        this.bookings = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Không thể tải danh sách booking';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  get filteredBookings(): Booking[] {
    if (this.filterStatus === 'all') {
      return this.bookings;
    }
    return this.bookings.filter(b => b.status === this.filterStatus);
  }

  cancelBooking(booking: Booking): void {
    const reason = prompt('Lý do hủy booking:');
    if (reason === null) return;

    this.adminService.updateBookingStatus(booking.id, 'Cancelled', reason).subscribe({
      next: () => {
        booking.status = 'Cancelled';
        booking.adminNote = reason;
        alert('Đã hủy booking thành công');
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra');
      }
    });
  }

  getStatusLabel(status: string): string {
    const labels: { [key: string]: string } = {
      'Pending': 'Chờ thanh toán',
      'Approved': 'Đã thanh toán',
      'Cancelled': 'Đã hủy',
      'Rejected': 'Từ chối'
    };
    return labels[status] || status;
  }

  getStatusClass(status: string): string {
    const classes: { [key: string]: string } = {
      'Pending': 'status-pending',
      'Approved': 'status-approved',
      'Cancelled': 'status-cancelled',
      'Rejected': 'status-rejected'
    };
    return classes[status] || '';
  }

  getDepositStatusLabel(status: string): string {
    const labels: { [key: string]: string } = {
      'Pending': 'Chưa TT',
      'Paid': 'Đã TT',
      'Failed': 'Thất bại',
      'Refunded': 'Hoàn tiền'
    };
    return labels[status] || status;
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('vi-VN');
  }

  formatDateTime(dateStr: string): string {
    return new Date(dateStr).toLocaleString('vi-VN');
  }

  formatCurrency(amount: number): string {
    return amount.toLocaleString('vi-VN') + ' VNĐ';
  }
}
