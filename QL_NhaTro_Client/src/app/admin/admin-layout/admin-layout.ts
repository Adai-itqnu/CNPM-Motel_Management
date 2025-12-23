import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css'
})
export class AdminLayoutComponent implements OnInit {
  currentUser: any = null;
  userInitials: string = '';
  showDropdown: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.currentUser = this.authService.getUser();
    if (this.currentUser?.fullName) {
      this.userInitials = this.getInitials(this.currentUser.fullName);
    }
  }

  getInitials(fullName: string): string {
    const names = fullName.trim().split(' ');
    if (names.length === 1) {
      return names[0].charAt(0).toUpperCase();
    }
    // Lấy chữ cái đầu của tên đầu và tên cuối
    return (names[0].charAt(0) + names[names.length - 1].charAt(0)).toUpperCase();
  }

  toggleDropdown() {
    this.showDropdown = !this.showDropdown;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  // Close dropdown when clicking outside
  closeDropdown(event: Event) {
    if (!(event.target as HTMLElement).closest('.user-profile')) {
      this.showDropdown = false;
    }
  }
}
