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

  username = '';
  email = '';
  password = '';
  confirmPassword = '';
  fullName = '';
  phone = '';

  message = '';
  loading = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onRegister() {
    // 1️⃣ Kiểm tra dữ liệu bắt buộc
    if (!this.username || !this.email || !this.password || !this.fullName) {
      this.message = 'Vui lòng nhập đầy đủ thông tin';
      return;
    }

    // 2️⃣ Kiểm tra mật khẩu
    if (this.password !== this.confirmPassword) {
      this.message = 'Mật khẩu xác nhận không khớp';
      return;
    }

    if (this.password.length < 6) {
      this.message = 'Mật khẩu phải từ 6 ký tự trở lên';
      return;
    }

    this.loading = true;
    this.message = '';

    // 3️⃣ Gọi API đăng ký
    this.authService.register(
      this.username,
      this.email,
      this.password,
      this.fullName,
      this.phone || undefined
    ).subscribe({
      next: () => {
        this.message = 'Đăng ký thành công! Chuyển sang đăng nhập...';

        // 4️⃣ Chuyển sang trang login
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 1500);
      },
      error: (err: any) => {
        this.message = err.error?.message || 'Đăng ký thất bại';
        this.loading = false;
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
}
