import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../services/user.service';
import { PasswordService } from '../../../services/password.service';
import { AuthService } from '../../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css'
})
export class UserProfileComponent implements OnInit {
  isLoading = false;
  isEditing = false;
  message = '';
  messageType: 'success' | 'error' = 'success';

  currentUser: any = null;
  profileForm = {
    fullName: '',
    email: '',
    phone: '',
    idCard: '',
    address: ''
  };

  // Password change
  showPasswordModal = false;
  passwordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  constructor(
    private userService: UserService,
    private passwordService: PasswordService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadProfile();
  }

  loadProfile() {
    this.currentUser = this.authService.getUser();
    
    // Immediately pre-fill form with current user data from localStorage
    if (this.currentUser) {
      this.profileForm = {
        fullName: this.currentUser.fullName || '',
        email: this.currentUser.email || '',
        phone: this.currentUser.phone || '',
        idCard: this.currentUser.idCard || '',
        address: this.currentUser.address || ''
      };
      this.isLoading = false;
    }
  }

  toggleEdit() {
    this.isEditing = !this.isEditing;
    this.message = '';
  }

  saveProfile() {
    this.isLoading = true;
    this.message = '';

    this.userService.updateUser(this.currentUser.id, this.profileForm).subscribe({
      next: (response) => {
        this.message = 'Cập nhật thông tin thành công!';
        this.messageType = 'success';
        this.isEditing = false;
        this.isLoading = false;
        
        // Update local storage
        const updatedUser = { ...this.currentUser, ...this.profileForm };
        localStorage.setItem('user', JSON.stringify(updatedUser));
        this.currentUser = updatedUser;
      },
      error: (err) => {
        this.message = 'Lỗi: ' + (err.error?.message || 'Không thể cập nhật thông tin');
        this.messageType = 'error';
        this.isLoading = false;
      }
    });
  }

  cancelEdit() {
    this.isEditing = false;
    this.message = '';
    // Reset form
    this.profileForm = {
      fullName: this.currentUser.fullName || '',
      email: this.currentUser.email || '',
      phone: this.currentUser.phone || '',
      idCard: this.currentUser.idCard || '',
      address: this.currentUser.address || ''
    };
  }

  deleteAccount() {
    if (!confirm('⚠️ Bạn có chắc chắn muốn xóa tài khoản? Hành động này không thể hoàn tác!')) {
      return;
    }

    if (!confirm('Xác nhận lần cuối: Tất cả dữ liệu của bạn sẽ bị xóa vĩnh viễn!')) {
      return;
    }

    // TODO: Implement delete account API
    alert('Chức năng xóa tài khoản đang được phát triển');
  }

  openPasswordModal() {
    this.showPasswordModal = true;
    this.message = '';
  }

  closePasswordModal() {
    this.showPasswordModal = false;
    this.passwordForm = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
  }

  changePassword() {
    if (!this.passwordForm.currentPassword || !this.passwordForm.newPassword) {
      this.message = 'Vui lòng điền đầy đủ thông tin';
      this.messageType = 'error';
      return;
    }

    if (this.passwordForm.newPassword.length < 6) {
      this.message = 'Mật khẩu mới phải có ít nhất 6 ký tự';
      this.messageType = 'error';
      return;
    }

    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      this.message = 'Mật khẩu xác nhận không khớp';
      this.messageType = 'error';
      return;
    }

    this.isLoading = true;
    this.message = '';

    this.passwordService.changePassword({
      currentPassword: this.passwordForm.currentPassword,
      newPassword: this.passwordForm.newPassword
    }).subscribe({
      next: (response) => {
        this.message = 'Đổi mật khẩu thành công!';
        this.messageType = 'success';
        this.isLoading = false;
        this.closePasswordModal();
      },
      error: (err: any) => {
        this.message = 'Lỗi: ' + (err.error?.message || 'Không thể đổi mật khẩu');
        this.messageType = 'error';
        this.isLoading = false;
      }
    });
  }

  getInitials(): string {
    if (!this.currentUser?.fullName) return 'U';
    const names = this.currentUser.fullName.trim().split(' ');
    if (names.length === 1) {
      return names[0].charAt(0).toUpperCase();
    }
    return (names[0].charAt(0) + names[names.length - 1].charAt(0)).toUpperCase();
  }
}
