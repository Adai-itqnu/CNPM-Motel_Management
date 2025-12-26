import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { Notification } from '../../models/notification.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-user-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './user-layout.html',
  styleUrl: './user-layout.css'
})
export class UserLayoutComponent implements OnInit, OnDestroy {
  currentUser: any = null;
  userInitials: string = '';
  showDropdown: boolean = false;
  
  // Notification
  showNotificationDropdown: boolean = false;
  unreadCount: number = 0;
  notifications: Notification[] = [];
  selectedNotification: Notification | null = null;
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

    // Subscribe to user changes to update avatar in real-time
    this.subscriptions.push(
      this.authService.user$.subscribe(user => {
        this.currentUser = user;
        if (user?.fullName) {
          this.userInitials = this.getInitials(user.fullName);
        }
      })
    );

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

  getInitials(fullName: string): string {
    const names = fullName.trim().split(' ');
    if (names.length === 1) {
      return names[0].charAt(0).toUpperCase();
    }
    return (names[0].charAt(0) + names[names.length - 1].charAt(0)).toUpperCase();
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

  viewNotificationDetail(notification: Notification) {
    // Mark as read
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe();
      notification.isRead = true;
    }
    
    // Show modal with full content
    this.selectedNotification = notification;
    this.showNotificationDropdown = false;
  }

  closeNotificationModal() {
    this.selectedNotification = null;
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

  navigateTo(path: string) {
    this.showDropdown = false;
    this.showNotificationDropdown = false;
    this.selectedNotification = null;
    this.router.navigate([path]);
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getAvatarUrl(): string {
    if (!this.currentUser?.avatarUrl) return '';
    if (this.currentUser.avatarUrl.startsWith('data:image')) return this.currentUser.avatarUrl;
    return `http://localhost:5001${this.currentUser.avatarUrl}?t=${Date.now()}`;
  }
}

