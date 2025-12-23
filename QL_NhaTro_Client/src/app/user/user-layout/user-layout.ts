import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-user-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './user-layout.html',
  styleUrl: './user-layout.css'
})
export class UserLayoutComponent implements OnInit {
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
    return (names[0].charAt(0) + names[names.length - 1].charAt(0)).toUpperCase();
  }

  toggleDropdown() {
    this.showDropdown = !this.showDropdown;
  }

  navigateTo(path: string) {
    this.showDropdown = false;
    this.router.navigate([path]);
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
