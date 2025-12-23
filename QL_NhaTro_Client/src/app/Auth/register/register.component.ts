import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  
  // Form fields
  fullName = '';
  email = '';
  phone = '';
  idCard = '';
  username = '';
  password = '';
  confirmPassword = '';
  agreeTerms = false;

  // UI states
  message = '';
  loading = false;
  showPassword = false;
  showConfirmPassword = false;

  constructor(
    private authService: AuthService,
    public router: Router
  ) {}

  // Password strength calculator
  getPasswordStrength(): { level: number; text: string; color: string } {
    if (!this.password) {
      return { level: 0, text: '', color: '' };
    }

    let strength = 0;
    if (this.password.length >= 8) strength++;
    if (/[a-z]/.test(this.password)) strength++;
    if (/[A-Z]/.test(this.password)) strength++;
    if (/[0-9]/.test(this.password)) strength++;
    if (/[^a-zA-Z0-9]/.test(this.password)) strength++;

    if (strength <= 2) {
      return { level: 1, text: 'Yếu', color: 'red' };
    } else if (strength === 3) {
      return { level: 2, text: 'Trung bình', color: 'yellow' };
    } else if (strength === 4) {
      return { level: 3, text: 'Mạnh', color: 'green' };
    } else {
      return { level: 4, text: 'Rất mạnh', color: 'blue' };
    }
  }

  // Check if passwords match
  get passwordsMatch(): boolean {
    return this.password === this.confirmPassword && this.confirmPassword.length > 0;
  }

  get passwordsDontMatch(): boolean {
    return this.password !== this.confirmPassword && this.confirmPassword.length > 0;
  }

  // Validate full name
  get fullNameValid(): boolean {
    return this.fullName.trim().length >= 3;
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPassword() {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onRegister() {
    // Basic validation
    if (!this.fullName || !this.email || !this.phone || !this.username || !this.password || !this.confirmPassword) {
      this.message = 'Vui lòng điền đầy đủ thông tin bắt buộc';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.message = 'Mật khẩu xác nhận không khớp';
      return;
    }

    if (!this.agreeTerms) {
      this.message = 'Vui lòng đồng ý với điều khoản sử dụng';
      return;
    }

    this.loading = true;
    this.message = '';

    this.authService.register(
      this.username,
      this.email,
      this.password,
      this.fullName,
      this.phone,
      this.idCard
    ).subscribe({
      next: (response) => {
        this.message = 'Đăng ký thành công! Chuyển hướng đến trang đăng nhập...';
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (err) => {
        this.message = err.error?.message || 'Đăng ký thất bại';
        this.loading = false;
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
}
