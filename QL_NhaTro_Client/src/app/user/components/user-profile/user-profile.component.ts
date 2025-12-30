import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../services/user.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css'
})
export class UserProfileComponent implements OnInit {

  // UI states
  isEditing = false;
  loading = false;
  message = '';
  messageType: 'success' | 'error' = 'success';

  // Current user from auth
  currentUser: any;

  // Form fields (like login/register)
  fullName = '';
  email = '';
  phone = '';
  idCard = '';
  address = '';

  // Avatar
  avatarFile: File | null = null;

  constructor(
    private userService: UserService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadUser();
  }

  loadUser() {
    this.currentUser = this.authService.getUser();
    if (!this.currentUser) return;

    // Load data vào các biến riêng lẻ (giống login/register)
    this.fullName = this.currentUser.fullName || '';
    this.email = this.currentUser.email || '';
    this.phone = this.currentUser.phone || '';
    this.idCard = this.currentUser.idCard || '';
    this.address = this.currentUser.address || '';
  }

  toggleEdit() {
    this.isEditing = !this.isEditing;
    this.message = '';
  }

  cancelEdit() {
    this.isEditing = false;
    this.avatarFile = null;
    this.loadUser();
  }

  save() {
    // Validation (giống login/register)
    if (!this.phone?.trim()) {
      this.show('❌ Vui lòng nhập số điện thoại', 'error');
      return;
    }
    if (!this.idCard?.trim()) {
      this.show('❌ Vui lòng nhập CCCD/CMND', 'error');
      return;
    }

    this.loading = true;  // Bật loading
    this.isEditing = false;

    if (this.avatarFile) {
      this.uploadAvatar();
    } else {
      this.saveProfile();
    }
  }

  uploadAvatar() {
    if (!this.avatarFile) return;

    const data = new FormData();
    data.append('file', this.avatarFile);

    this.userService.uploadAvatar(data).subscribe({
      next: (res: any) => {
        if (this.currentUser) {
          this.currentUser.avatarUrl = res.avatarUrl;
        }
        this.avatarFile = null;
        this.saveProfile();
      },
      error: () => {
        this.loading = false;  // Tắt loading khi lỗi
        this.show('❌ Upload ảnh thất bại', 'error');
      }
    });
  }

  saveProfile() {
    // Tạo object để gửi lên server (giống login/register style)
    const profileData = {
      fullName: this.fullName,
      email: this.email,
      phone: this.phone,
      idCard: this.idCard,
      address: this.address
    };

    // BƯỚC 1: Gọi API update DB
    this.userService.updateMyProfile(profileData).subscribe({
      next: () => {
        // BƯỚC 2: BE trả success
        // BƯỚC 3 & 4: FE update RAM (BehaviorSubject) và localStorage
        const updatedUser = { ...this.currentUser, ...profileData };
        this.authService.updateUser(updatedUser);
        this.currentUser = updatedUser;

        this.loading = false;  // Tắt loading
        this.show('✅ Lưu thành công', 'success');
      },
      error: () => {
        this.loading = false;  // Tắt loading khi lỗi
        this.show('❌ Lỗi khi lưu', 'error');
      }
    });
  }

  onFileChange(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    // Validate file type
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
    if (!validTypes.includes(file.type)) {
      this.show('❌ Chỉ chấp nhận ảnh (.jpg, .png, .gif)', 'error');
      return;
    }

    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      this.show('❌ File quá lớn (tối đa 5MB)', 'error');
      return;
    }

    this.avatarFile = file;

    const reader = new FileReader();
    reader.onload = (e: any) => {
      if (this.currentUser && e.target?.result) {
        this.currentUser.avatarUrl = e.target.result;
      }
    };
    reader.readAsDataURL(file);
  }

  getAvatar(): string {
    if (!this.currentUser?.avatarUrl) return '';
    if (this.currentUser.avatarUrl.startsWith('data:image')) {
      return this.currentUser.avatarUrl;
    }
    // Thêm timestamp để tránh cache ảnh cũ
    return `http://localhost:5001${this.currentUser.avatarUrl}?t=${Date.now()}`;
  }

  getInitials(): string {
    if (!this.currentUser?.fullName) return 'U';
    const name = this.currentUser.fullName.split(' ');
    return name.length === 1
      ? name[0][0].toUpperCase()
      : (name[0][0] + name[name.length - 1][0]).toUpperCase();
  }

  show(text: string, type: 'success' | 'error') {
    this.message = text;
    this.messageType = type;
    setTimeout(() => this.message = '', 3000);
  }
}
