import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  usernameOrEmail = '';
  password = '';
  message = '';
  loading = false;
  showPassword = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  onLogin() {
    if (!this.usernameOrEmail || !this.password) {
      this.message = 'Vui lòng nhập tài khoản và mật khẩu';
      return;
    }

    this.loading = true;
    this.message = '';

    this.authService.login(this.usernameOrEmail, this.password)
      .subscribe({
        next: () => {
          if (this.authService.isAdmin()) {
            this.router.navigate(['/admin/dashboard']);
          } else {
            this.router.navigate(['/user/dashboard']);
          }
        },
        error: (err) => {
          this.message = err.error?.message || 'Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin đăng nhập';
          this.loading = false;
        },
        complete: () => {
          this.loading = false;
        }
      });
  }
}
