import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { Notification } from '../../models/notification.model';
import { Subscription, filter } from 'rxjs';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css'
})
export class AdminLayoutComponent implements OnInit, OnDestroy {
  currentUser: any = null;
  userInitials: string = '';
  showDropdown: boolean = false;
  sidebarCollapsed: boolean = false;
  pageTitle: string = 'Dashboard';
  
  // Notification
  showNotificationDropdown: boolean = false;
  unreadCount: number = 0;
  notifications: Notification[] = [];
  
  private subscriptions: Subscription[] = [];

  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.currentUser = this.authService.getUser();
    if (this.currentUser?.fullName) {
      this.userInitials = this.getInitials(this.currentUser.fullName);
    }

    // Listen to route changes to update page title
    this.subscriptions.push(
      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe(() => {
        this.updatePageTitle();
      })
    );
    this.updatePageTitle();

    // Subscribe to unread count
    this.subscriptions.push(
      this.notificationService.unreadCount$.subscribe(count => {
        this.unreadCount = count;
      })
    );

    // Load initial unread count
    this.notificationService.refreshUnreadCount();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  updatePageTitle() {
    const url = this.router.url;
    const titles: { [key: string]: string } = {
      '/admin/dashboard': 'Tổng quan Dashboard',
      '/admin/rooms': 'Quản lý Phòng trọ',
      '/admin/bookings': 'Quản lý Đặt phòng',
      '/admin/contracts': 'Quản lý Hợp đồng',
      '/admin/users': 'Quản lý Người thuê',
      '/admin/bills': 'Quản lý Hóa đơn',
      '/admin/payments': 'Lịch sử Thanh toán',
      '/admin/notifications': 'Gửi Thông báo'
    };
    this.pageTitle = titles[url] || 'Dashboard';
  }

  getInitials(fullName: string): string {
    const names = fullName.trim().split(' ');
    if (names.length === 1) {
      return names[0].charAt(0).toUpperCase();
    }
    return (names[0].charAt(0) + names[names.length - 1].charAt(0)).toUpperCase();
  }

  toggleSidebar() {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  toggleDropdown() {
    this.showDropdown = !this.showDropdown;
    this.showNotificationDropdown = false;
  }

  toggleNotificationDropdown() {
    this.showNotificationDropdown = !this.showNotificationDropdown;
    this.showDropdown = false;
    
    if (this.showNotificationDropdown && this.notifications.length === 0) {
      this.loadNotifications();
    }
  }

  loadNotifications() {
    this.notificationService.getNotifications(1, 5).subscribe({
      next: (res) => {
        this.notifications = res.notifications;
      },
      error: (err) => console.error('Error loading notifications:', err)
    });
  }

  markAsRead(notification: Notification) {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe();
      notification.isRead = true;
    }
    
    if (notification.link) {
      this.showNotificationDropdown = false;
      this.router.navigate([notification.link]);
    }
  }

  markAllAsRead() {
    this.notificationService.markAllAsRead().subscribe(() => {
      this.notifications.forEach(n => n.isRead = true);
    });
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'Payment': return '💰';
      case 'Warning': return '⚠️';
      case 'Admin': return '📢';
      default: return '🔔';
    }
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  // Close dropdowns when clicking outside
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    const target = event.target as HTMLElement;
    if (!target.closest('.user-profile-mini') && !target.closest('.dropdown-menu')) {
      this.showDropdown = false;
    }
    if (!target.closest('.notification-bell') && !target.closest('.notification-dropdown')) {
      this.showNotificationDropdown = false;
    }
  }
}
