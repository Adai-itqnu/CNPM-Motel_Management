import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoomService } from '../../../services/room.service';
import { BookingService } from '../../../services/booking.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-dashboard.component.html',
  styleUrl: './user-dashboard.component.css'
})
export class UserDashboardComponent implements OnInit {
  isLoading = true;
  currentUser: any = null;
  
  // Rooms data
  availableRooms: any[] = [];
  userBooking: any = null;
  
  // Filters
  selectedFilter = 'all';
  filters = [
    { value: 'all', label: 'Tất cả' },
    { value: 'under3', label: 'Dưới 3tr' },
    { value: '3to5', label: '3tr - 5tr' },
    { value: 'balcony', label: 'Có ban công' }
  ];
  
  // Room Detail Modal
  selectedRoom: any = null;
  showRoomDetail = false;

  // Booking Modal
  showBookingModal = false;
  bookingForm = {
    contactPhone: '',
    contactEmail: '',
    checkInDate: '',
    notes: ''
  };

  constructor(
    private roomService: RoomService,
    private bookingService: BookingService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.currentUser = this.authService.getUser();
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading = true;
    
    // Load available rooms
    this.roomService.getRooms({ 
      status: 'available',
      pageSize: 20 
    }).subscribe({
      next: (response) => {
        this.availableRooms = response.data || [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading rooms:', err);
        this.isLoading = false;
      }
    });
  }

  filterRooms(filter: string) {
    this.selectedFilter = filter;
    // Apply filtering logic here based on filter value
  }

  viewRoomDetail(room: any) {
    this.selectedRoom = room;
    this.showRoomDetail = true;
  }

  closeRoomDetail() {
    this.showRoomDetail = false;
    this.selectedRoom = null;
  }

  // Open booking modal
  openBookingModal() {
    // Check if user has CCCD and Phone
    if (!this.currentUser?.idCard || !this.currentUser?.phone) {
      let missing = [];
      if (!this.currentUser?.idCard) missing.push('CCCD');
      if (!this.currentUser?.phone) missing.push('Số điện thoại');
      
      alert(`⚠️ Vui lòng cập nhật ${missing.join(' và ')} trong thông tin cá nhân trước khi đặt phòng!`);
      return;
    }

    this.showBookingModal = true;
    this.showRoomDetail = false;
    
    // Pre-fill contact info from user
    this.bookingForm.contactEmail = this.currentUser?.email || '';
    this.bookingForm.contactPhone = this.currentUser?.phone || '';
    this.bookingForm.checkInDate = this.getTomorrowDate();
  }

  closeBookingModal() {
    this.showBookingModal = false;
    this.bookingForm = {
      contactPhone: '',
      contactEmail: '',
      checkInDate: '',
      notes: ''
    };
  }

  // Submit booking and redirect to VNPAY
  submitBooking() {
    if (!this.selectedRoom) return;

    if (!this.bookingForm.contactPhone || !this.bookingForm.contactEmail) {
      alert('Vui lòng điền đầy đủ thông tin liên hệ!');
      return;
    }

    const bookingData = {
      roomId: this.selectedRoom.id,
      checkInDate: this.bookingForm.checkInDate,
      contactPhone: this.bookingForm.contactPhone,
      contactEmail: this.bookingForm.contactEmail,
      notes: this.bookingForm.notes
    };


    this.bookingService.createDepositBooking(bookingData).subscribe({
      next: (response) => {
        // Redirect to VNPAY payment
        window.location.href = response.paymentUrl;
      },
      error: (err) => {
        if (err.error?.requireUpdate) {
          alert(err.error.message);
        } else {
          alert('Lỗi: ' + (err.error?.message || 'Không thể tạo booking'));
        }
      }
    });
  }

  formatPrice(price: number): string {
    return `${(price / 1000000).toFixed(1)} tr`;
  }

  getAmenitiesCount(room: any): number {
    return room.amenities?.length || 0;
  }

  getTomorrowDate(): string {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow.toISOString().split('T')[0];
  }
}
