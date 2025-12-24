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

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPassword() {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onRegister() {
    this.message = '';

    // Basic validation
    if (!this.fullName?.trim() || !this.email?.trim() || !this.phone?.trim() || !this.username?.trim() || !this.password || !this.confirmPassword) {
      this.message = 'Vui lòng điền đầy đủ thông tin bắt buộc';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.message = 'Mật khẩu xác nhận không khớp';
      return;
    }

    if (this.password.length < 6) {
      this.message = 'Mật khẩu phải có ít nhất 6 ký tự';
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
        this.message = 'Đăng ký thành công!';
        this.router.navigate(['/login']);
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
