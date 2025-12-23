import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoomService } from '../../../services/room.service';
import { Room, CreateRoomDto, UpdateRoomDto, RoomImage } from '../../../models/room.model';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-room-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './room-management.component.html',
  styleUrl: './room-management.component.css'
})
export class RoomManagementComponent implements OnInit {
  rooms: any[] = [];
  loading = false;
  message = '';
  
  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  total = 0;

  // Search
  searchText = '';

  // Modal
  showModal = false;
  modalMode: 'create' | 'edit' | 'view' = 'create';
  selectedRoom: any = null;

  // Form
  roomForm: CreateRoomDto = {
    name: '',
    roomType: '',
    price: 0,
    depositAmount: 0,
    electricityPrice: 3500,
    waterPrice: 20000,
    description: '',
    area: 0,
    floor: 1,
    amenities: []
  };

  // Image upload
  selectedFiles: File[] = [];
  uploadProgress: { [key: string]: number } = {};
  isUploading = false;
  newAmenity = '';
  maxImages = 5; // Giới hạn tối đa 5 ảnh

  // Room type options
  roomTypes = [
    { value: 'single', label: 'Phòng đơn' },
    { value: 'double', label: 'Phòng đôi' },
    { value: 'vip', label: 'Phòng VIP' },
    { value: 'studio', label: 'Phòng studio' }
  ];

  constructor(private roomService: RoomService) {}

  ngOnInit() {
    this.loadRooms();
  }

  loadRooms() {
    this.loading = true;
    this.roomService.getRooms({
      search: this.searchText || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        this.rooms = response.data || [];
        this.total = response.total || 0;
        this.totalPages = response.totalPages || 1;
        this.loading = false;
      },
      error: (err) => {
        this.message = 'Lỗi khi tải danh sách phòng: ' + (err.error?.message || 'Vui lòng thử lại sau');
        this.loading = false;
        console.error('Error loading rooms:', err);
      }
    });
  }

  onSearch() {
    this.currentPage = 1;
    this.loadRooms();
  }

  changePage(page: number) {
    this.currentPage = page;
    this.loadRooms();
  }

  openCreateModal() {
    this.modalMode = 'create';
    this.resetForm();
    this.showModal = true;
  }

  openEditModal(room: any) {
    this.modalMode = 'edit';
    this.selectedRoom = room;
    this.roomForm = {
      name: room.name,
      roomType: room.roomType,
      price: room.price,
      depositAmount: room.depositAmount,
      electricityPrice: room.electricityPrice,
      waterPrice: room.waterPrice,
      description: room.description,
      area: room.area,
      floor: room.floor,
      amenities: [...room.amenities]
    };
    this.showModal = true;
  }

  openViewModal(room: any) {
    this.modalMode = 'view';
    this.selectedRoom = room;
    this.showModal = true;
    
    // Load full room details including images if not already loaded
    if (!this.selectedRoom.images || this.selectedRoom.images.length === 0) {
      this.roomService.getRoom(room.id).subscribe({
        next: (roomDetails) => {
          this.selectedRoom = { ...this.selectedRoom, ...roomDetails };
        },
        error: (err) => {
          console.error('Error loading room details:', err);
        }
      });
    }
  }

  closeModal() {
    this.showModal = false;
    this.resetForm();
    this.loading = false; // Ensure loading is reset
    this.message = ''; // Clear any messages
  }

  resetForm() {
    this.roomForm = {
      name: '',
      roomType: '',
      price: 0,
      depositAmount: 0,
      electricityPrice: 3500,
      waterPrice: 20000,
      description: '',
      area: 0,
      floor: 1,
      amenities: []
    };
    this.selectedFiles = [];
    this.uploadProgress = {};
    this.newAmenity = '';
  }

  addAmenityToForm() {
    if (this.newAmenity.trim()) {
      if (!this.roomForm.amenities) this.roomForm.amenities = [];
      this.roomForm.amenities.push(this.newAmenity.trim());
      this.newAmenity = '';
    }
  }

  removeAmenityFromForm(index: number) {
    this.roomForm.amenities?.splice(index, 1);
  }

  onSubmit() {
    if (this.modalMode === 'create') {
      this.createRoom();
    } else if (this.modalMode === 'edit') {
      this.updateRoom();
    }
  }

  createRoom() {
    if (this.loading) return; // Prevent multiple submissions
    
    this.loading = true;
    this.message = ''; // Clear any previous messages
    
    this.roomService.createRoom(this.roomForm).pipe(
      finalize(() => {
        this.loading = false;
      })
    ).subscribe({
      next: (response: any) => {
        const roomId = response.roomId;
        
        // Upload images if any selected
        if (this.selectedFiles.length > 0 && roomId) {
          this.message = 'Đang tải ảnh lên...';
          this.uploadRoomImages(roomId).then(() => {
            this.message = 'Tạo phòng và tải ảnh thành công!';
            this.closeModal();
            this.loadRooms();
          }).catch(() => {
            this.message = 'Tạo phòng thành công nhưng có lỗi khi tải ảnh';
            setTimeout(() => {
              this.closeModal();
              this.loadRooms();
            }, 2000);
          });
        } else {
          this.message = 'Tạo phòng thành công';
          this.closeModal();
          this.loadRooms();
        }
      },
      error: (err) => {
        console.error('Error creating room:', err);
        this.message = err.error?.message || 'Đã xảy ra lỗi khi tạo phòng. Vui lòng thử lại.';
      }
    });
  }

  // Helper to get file preview URL
  getFilePreview(file: File): string {
    return URL.createObjectURL(file);
  }

  updateRoom() {
    if (!this.selectedRoom) return;
    
    this.loading = true;
    const updateData: UpdateRoomDto = {
      name: this.roomForm.name,
      roomType: this.roomForm.roomType,
      price: this.roomForm.price,
      depositAmount: this.roomForm.depositAmount,
      electricityPrice: this.roomForm.electricityPrice,
      waterPrice: this.roomForm.waterPrice,
      description: this.roomForm.description,
      area: this.roomForm.area,
      floor: this.roomForm.floor
    };

    this.roomService.updateRoom(this.selectedRoom.id, updateData).subscribe({
      next: () => {
        this.message = 'Cập nhật phòng thành công';
        this.closeModal();
        this.loadRooms();
      },
      error: (err) => {
        this.message = err.error?.message || 'Lỗi khi cập nhật phòng';
        this.loading = false;
      }
    });
  }

  deleteRoom(room: any) {
    if (!confirm(`Bạn có chắc muốn xóa phòng "${room.name}"?`)) return;

    this.loading = true;
    this.roomService.deleteRoom(room.id).subscribe({
      next: () => {
        this.message = 'Xóa phòng thành công';
        this.loadRooms();
      },
      error: (err) => {
        this.message = err.error?.message || 'Lỗi khi xóa phòng';
        this.loading = false;
      }
    });
  }

  getStatusBadgeClass(status: string): string {
    switch(status.toLowerCase()) {
      case 'available': return 'badge-success';
      case 'occupied': return 'badge-danger';
      case 'maintenance': return 'badge-warning';
      case 'reserved': return 'badge-info';
      default: return 'badge-secondary';
    }
  }

  // Image handling
  onFileSelected(event: any) {
    const files: FileList = event.target.files;
    if (files && files.length > 0) {
      // Check total images count (existing + new)
      const totalImages = this.selectedFiles.length + files.length;
      if (totalImages > this.maxImages) {
        this.message = `Chỉ được chọn tối đa ${this.maxImages} ảnh. Bạn đã chọn ${this.selectedFiles.length} ảnh.`;
        return;
      }

      for (let i = 0; i < files.length; i++) {
        const file = files.item(i);
        if (file) {
          // Check if file is an image
          if (!file.type.match('image.*')) {
            this.message = 'Chỉ chấp nhận file ảnh';
            continue;
          }
          
          // Check file size (max 5MB)
          if (file.size > 5 * 1024 * 1024) {
            this.message = `File ${file.name} vượt quá kích thước cho phép (5MB)`;
            continue;
          }
          
          this.selectedFiles.push(file);
        }
      }
    }
  }

  removeSelectedFile(index: number) {
    this.selectedFiles.splice(index, 1);
  }

  uploadRoomImages(roomId: string) {
    if (this.selectedFiles.length === 0) return Promise.resolve();

    this.isUploading = true;
    const uploadPromises = this.selectedFiles.map(file => {
      return new Promise<void>((resolve, reject) => {
        this.roomService.uploadRoomImage(roomId, file).subscribe({
          next: () => resolve(),
          error: (err) => {
            console.error('Upload error:', err);
            this.message = `Lỗi khi tải lên ảnh ${file.name}`;
            reject(err);
          }
        });
      });
    });

    return Promise.all(uploadPromises)
      .then(() => {
        this.selectedFiles = [];
        this.isUploading = false;
      })
      .catch(() => {
        this.isUploading = false;
      });
  }

  deleteImage(roomId: string, imageId: number, event: Event) {
    event.stopPropagation();
    if (!confirm('Bạn có chắc chắn muốn xóa ảnh này?')) return;

    this.roomService.deleteImage(roomId, imageId).subscribe({
      next: () => {
        if (this.selectedRoom) {
          this.selectedRoom.images = this.selectedRoom.images.filter((img: RoomImage) => img.id !== imageId);
        }
        this.message = 'Đã xóa ảnh thành công';
      },
      error: (err) => {
        console.error('Error deleting image:', err);
        this.message = 'Lỗi khi xóa ảnh';
      }
    });
  }

  setPrimaryImage(roomId: string, imageId: number, event: Event) {
    event.stopPropagation();
    this.roomService.setPrimaryImage(roomId, imageId).subscribe({
      next: () => {
        if (this.selectedRoom) {
          // Update primary image status
          this.selectedRoom.images = this.selectedRoom.images.map((img: RoomImage) => ({
            ...img,
            isPrimary: img.id === imageId
          }));
        }
        this.message = 'Đã đặt làm ảnh đại diện';
      },
      error: (err) => {
        console.error('Error setting primary image:', err);
        this.message = 'Lỗi khi đặt ảnh đại diện';
      }
    });
  }

  getStatusText(status: string): string {
    switch(status.toLowerCase()) {
      case 'available': return 'Trống';
      case 'occupied': return 'Đã thuê';
      case 'maintenance': return 'Bảo trì';
      case 'reserved': return 'Đã đặt';
      default: return status;
    }
  }
  // Quản lý phòng
  // CRUD: Thêm, sửa, xóa phòng
  // Cập nhật thông tin: giá, diện tích, tiện nghi, hình ảnh
  // Thống kê: tỷ lệ lấp đầy, phòng trống, phòng bảo trì
}
