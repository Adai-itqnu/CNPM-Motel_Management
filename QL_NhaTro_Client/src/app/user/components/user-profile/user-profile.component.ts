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
  currentUser: any;

  form = {
    fullName: '',
    email: '',
    phone: '',
    idCard: '',
    address: ''
  };

  avatarFile: File | null = null;
  message = '';
  messageType: 'success' | 'error' = 'success';

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

    this.form = {
      fullName: this.currentUser.fullName || '',
      email: this.currentUser.email || '',
      phone: this.currentUser.phone || '',
      idCard: this.currentUser.idCard || '',
      address: this.currentUser.address || ''
    };
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
    // Validate từng field cụ thể
    if (!this.form.phone?.trim()) {
      this.show('❌ Vui lòng nhập số điện thoại', 'error');
      return;
    }
    if (!this.form.idCard?.trim()) {
      this.show('❌ Vui lòng nhập CCCD/CMND', 'error');
      return;
    }

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
      error: () => this.show('❌ Upload ảnh thất bại', 'error')
    });
  }

  saveProfile() {
    const updatedUser = { ...this.currentUser, ...this.form };

    this.authService.updateUser(updatedUser);
    this.currentUser = updatedUser;

    this.userService.updateMyProfile(this.form).subscribe({
      next: () => this.show('✅ Lưu thành công', 'success'),
      error: () => {
        this.loadUser(); // Khôi phục data cũ khi lỗi
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
