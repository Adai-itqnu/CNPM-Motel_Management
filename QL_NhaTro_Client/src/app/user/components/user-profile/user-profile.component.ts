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
  isEditing = false;
  message = '';
  messageType: 'success' | 'error' = 'success';
  phoneError = '';
  idCardError = '';
  currentUser: any = null;
  profileForm = { fullName: '', email: '', phone: '', idCard: '', address: '' };
  selectedAvatarFile: File | null = null;

  constructor(private userService: UserService, private authService: AuthService) {}

  ngOnInit() {
    this.loadProfile();
  }

  loadProfile() {
    this.currentUser = this.authService.getUser();
    if (this.currentUser) this.profileForm = { ...this.currentUser };
  }

  toggleEdit() {
    this.isEditing = !this.isEditing;
    this.message = '';
    this.phoneError = '';
    this.idCardError = '';
  }

  saveProfile() {
    this.message = '';
    this.phoneError = '';
    this.idCardError = '';

    if (!this.profileForm.phone?.trim()) {
      this.phoneError = 'Số điện thoại là bắt buộc';
      return;
    }
    if (!this.profileForm.idCard?.trim()) {
      this.idCardError = 'Số CCCD/CMND là bắt buộc';
      return;
    }

    this.isEditing = false;

    if (this.selectedAvatarFile) {
      const formData = new FormData();
      formData.append('file', this.selectedAvatarFile);
      
      this.userService.uploadAvatar(formData).subscribe({
        next: (res: any) => {
          this.currentUser.avatarUrl = res.avatarUrl;
          this.selectedAvatarFile = null;
          this.saveProfileData();
        },
        error: () => {
          this.message = '❌ Lỗi upload ảnh';
          this.messageType = 'error';
        }
      });
    } else {
      this.saveProfileData();
    }
  }

  saveProfileData() {
    const updated = { ...this.currentUser, ...this.profileForm };
    localStorage.setItem('user', JSON.stringify(updated));
    this.currentUser = { ...updated };

    this.userService.updateMyProfile(this.profileForm).subscribe({
      next: () => {
        this.message = '✅ Lưu thành công!';
        this.messageType = 'success';
        setTimeout(() => this.message = '', 3000);
      },
      error: () => {
        this.loadProfile();
        this.message = '❌ Lỗi khi lưu';
        this.messageType = 'error';
      }
    });
  }

  cancelEdit() {
    this.isEditing = false;
    this.selectedAvatarFile = null;
    this.loadProfile();
    this.message = '';
    this.phoneError = '';
    this.idCardError = '';
  }

  triggerFileInput() {
    document.getElementById('avatarInput')?.click();
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    const validExts = ['.jpg', '.jpeg', '.png', '.gif'];
    if (!validExts.some(ext => file.name.toLowerCase().endsWith(ext))) {
      this.message = '❌ Chỉ chấp nhận ảnh';
      this.messageType = 'error';
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.message = '❌ File quá lớn';
      this.messageType = 'error';
      return;
    }

    this.selectedAvatarFile = file;
    const reader = new FileReader();
    reader.onload = (e: any) => {
      if (this.currentUser) this.currentUser.avatarUrl = e.target.result;
    };
    reader.readAsDataURL(file);
    
    this.message = '📷 Ảnh đã chọn. Nhấn Lưu để cập nhật.';
    this.messageType = 'success';
  }

  getAvatarUrl(): string {
    if (!this.currentUser?.avatarUrl) return '';
    if (this.currentUser.avatarUrl.startsWith('data:image')) return this.currentUser.avatarUrl;
    return `http://localhost:5001${this.currentUser.avatarUrl}?t=${Date.now()}`;
  }

  getInitials(): string {
    if (!this.currentUser?.fullName) return 'U';
    const names = this.currentUser.fullName.trim().split(' ');
    return names.length === 1 
      ? names[0][0].toUpperCase() 
      : (names[0][0] + names[names.length - 1][0]).toUpperCase();
  }
}
